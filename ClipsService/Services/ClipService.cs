using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;
using Microsoft.Azure.Cosmos;

namespace ClipsService.Services;

public class ClipService : IClipsService
{
    private readonly Container _clipsContainer;
    private readonly string _contianerId = "";

    public ClipService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        var dbName = Environment.GetEnvironmentVariable("CosmosDbCosmosDbId");
        _contianerId = Environment.GetEnvironmentVariable("CosmosDbClipsContainerId");

        var db = cosmosClient.GetDatabase(dbName);
        _clipsContainer = db.GetContainer(_contianerId);
    }

    public async Task<ServiceResult<List<Clip>>> GetClipsOfUser(string userId)
    {
        ServiceResult<List<Clip>> serviceResult = new();
        try
        {
            var queryDefinition = new QueryDefinition($"SELECT * FROM {_contianerId}");
            var iterator = _clipsContainer.GetItemQueryIterator<Clip>(
                queryDefinition,
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(userId)
                });
            var results = new List<Clip>();
            while(iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync();
                results.AddRange(result.Resource);
            }
            serviceResult.Result = results;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> AddClip(string userId, AddClipRequestDto addClipRequestDto)
    {
        ServiceResult<Clip> serviceResult = new();
        try
        {
            //@TODO Apply Validation Logic?
            var clip = new Clip()
            {
                Id = Guid.NewGuid().ToString(),
                Name = addClipRequestDto.Name,
                Description = addClipRequestDto.Description,
                Uri = addClipRequestDto.Uri == "" ? null : new Uri(addClipRequestDto.Uri),
                Converted = addClipRequestDto.Converted,
                Public = addClipRequestDto.Public,
                UserId = userId,
            };

            var creatNewClipResponse = await _clipsContainer.CreateItemAsync(clip, new PartitionKey(clip.UserId));
            if (creatNewClipResponse.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception($"Unable to Create new clip: User: {clip.Id} , Clip: {clip.Id}");

            serviceResult.Result = clip;
        }
        catch (Exception ex) 
        {
            serviceResult.IsError= true;
            serviceResult.ErrorMessage= ex.ToString();
        }

        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> UpdateClip(string userId, string clipId, UpdateClipRequestDto updateClipRequestDto)
    {

        ServiceResult<Clip> serviceResult = new();
        try
        {
            var oldClip = await GetClip(userId, clipId);

            var newClip = new Clip()
            {
                Id = oldClip.Id,
                Name = updateClipRequestDto.Name,
                Description = updateClipRequestDto.Description,
                Public = updateClipRequestDto.Public,
                UserId = oldClip.UserId,
                Uri = oldClip.Uri,
                Converted = oldClip.Converted,
            };
            newClip.DateModified = DateTime.Now;
            newClip.DateCreated = oldClip.DateCreated;

            var updateUserResponse = await _clipsContainer.ReplaceItemAsync(newClip, newClip.Id, new PartitionKey(newClip.UserId));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update clip: User: {userId} , Clip: {clipId}");

            serviceResult.Result = newClip;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }

        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> UpdateClipUri(string userId, string clipId, UpdateClipUriRequestDto updateClipUriRequestDto)
    {

        ServiceResult<Clip> serviceResult = new();
        try
        {
            var oldClip = await GetClip(userId, clipId);

            var newClip = new Clip()
            {
                Id = oldClip.Id,
                Name = oldClip.Name,
                Description = oldClip.Description,
                Public = oldClip.Public,
                UserId = oldClip.UserId,
                Uri = updateClipUriRequestDto.Uri,
                Converted = updateClipUriRequestDto.Converted,
            };
            newClip.DateModified = oldClip.DateModified;
            newClip.DateCreated = oldClip.DateCreated;

            var updateUserResponse = await _clipsContainer.ReplaceItemAsync(newClip, newClip.Id, new PartitionKey(newClip.UserId));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update clip uri: User: {userId} , Clip: {clipId}");

            serviceResult.Result = newClip;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }

        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> DeleteClip(string userId, string clipId)
    {

        ServiceResult<Clip> serviceResult = new();
        try
        {
            var deleteItemResponse = await _clipsContainer.DeleteItemAsync<Clip>(clipId, new PartitionKey(userId));
            if (deleteItemResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to remove clip: User: {userId} , Clip: {clipId}");
            serviceResult.Result = deleteItemResponse.Resource;
        }
        catch (Exception ex)
        {
            serviceResult.IsError= true;
            serviceResult.ErrorMessage= ex.ToString();
        }
        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> GetPublicClip(string userId, string clipId)

    {
        ServiceResult<Clip> serviceResult = new();
        try
        {
            var clip = await GetClip(userId, clipId);
            if (clip == null)
                throw new Exception($"Unable to find clip: ClipId: {clipId} userId: {userId}");
            serviceResult.Result = clip;
            if (clip.Public == false)
            {
                serviceResult.Result = null;
            }
        }
        catch (Exception ex)
        {
            serviceResult.IsError= true;
            serviceResult.ErrorMessage= ex.ToString();
        }
        return serviceResult;
    }

    private async Task<Clip> GetClip(string userId,string clipId)
    {
        var clipReadResponse = await _clipsContainer.ReadItemAsync<Clip>(clipId, new PartitionKey(userId));
        if (clipReadResponse.StatusCode != System.Net.HttpStatusCode.OK) 
            throw new Exception($"Unable to find clip: ClipId: {clipId} userId: {userId}");
        return clipReadResponse.Resource;
    }
}
