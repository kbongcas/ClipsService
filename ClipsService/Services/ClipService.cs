using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Diagnostics;
using User = ClipsService.Models.User;

namespace ClipsService.Services;

public class ClipService : IClipsService
{
    private readonly Container _usersContainer;

    public ClipService(IConfiguration configuration, CosmosClient cosmosClient)
    {
        /*
        var dbName = configuration.GetValue<string>("CosmosDb:CosmosDbId");
        var usersContianerId = configuration.GetValue<string>("CosmosDb:UsersContainerId");
        */
        var dbName = Environment.GetEnvironmentVariable("CosmosDbCosmosDbId");
        var usersContianerId = Environment.GetEnvironmentVariable("CosmosDbUsersContainerId");

        var db = cosmosClient.GetDatabase(dbName);
        _usersContainer = db.GetContainer(usersContianerId);
    }

    public async Task<ServiceResult<List<Clip>>> GetClips(string userId)
    {
        ServiceResult<List<Clip>> serviceResult = new();
        try
        {
            var user = await GetUser(userId);
            serviceResult.Result = user.Clips;
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
            var clip = new Clip(
                Guid.NewGuid().ToString(),
                addClipRequestDto.Name,
                addClipRequestDto.Description,
                addClipRequestDto.Uri == "" ? null : new Uri(addClipRequestDto.Uri),
                addClipRequestDto.Converted);

            var user = await GetUser(userId);

            user.Clips.Add(clip);

            var updateUserResponse = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update user's clips: User: {user.Id} , Clip: {clip.Id}");

            serviceResult.Result = clip;
        }
        catch (Exception ex) 
        {
            serviceResult.IsError= true;
            serviceResult.ErrorMessage= ex.ToString();
        }

        return serviceResult;
    }

    public async Task<ServiceResult<Clip>> UpdateClip(string userId, string clipId, UpdateMyClipRequestDto updateMyClipRequestDto)
    {

        ServiceResult<Clip> serviceResult = new();
        try
        {
            var user = await GetUser(userId);
            var index = user.Clips.FindIndex(x => x.Id == clipId);
            if (index == -1)
                throw new Exception($"Unable to update user's clip, clip not found: User: {user.Id} , Clip: {clipId}");

            var oldClip = user.Clips[index];
            var newClip = new Clip(
                clipId,
                updateMyClipRequestDto.Name,
                updateMyClipRequestDto.Description,
                oldClip.Uri,
                oldClip.Converted
                ); ;
            user.Clips[index] = newClip;

            var updateUserResponse = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update user's clips: User: {user.Id} , Clip: {clipId}");

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
            var user = await GetUser(userId);
            var index = user.Clips.FindIndex(x => x.Id == clipId);
            if (index == -1)
                throw new Exception($"Unable to update user's clip, clip not found: User: {user.Id} , Clip: {clipId}");

            var oldClip = user.Clips[index];
            var newClip = new Clip(
                clipId,
                oldClip.Name,
                oldClip.Description,
                updateClipUriRequestDto.Uri,
                updateClipUriRequestDto.Converted
                );
            user.Clips[index] = newClip;

            var updateUserResponse = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update user's clips uri: User: {user.Id} , Clip: {clipId}");

            serviceResult.Result = newClip;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }

        return serviceResult;
    }


    public async Task<ServiceResult<Clip>> UpdateClip(string userId, string clipId, UpdateClipRequestDto updateClipRequestDto)
    {

        ServiceResult<Clip> serviceResult = new();
        try
        {
            var user = await GetUser(userId);
            var index = user.Clips.FindIndex(x => x.Id == clipId);
            if (index == -1)
                throw new Exception($"Unable to update user's clip, clip not found: User: {user.Id} , Clip: {clipId}");

            var oldClip = user.Clips[index];
            var newClip = new Clip(
                clipId,
                updateClipRequestDto.Name,
                updateClipRequestDto.Description,
                updateClipRequestDto.Uri,
                updateClipRequestDto.Converted
                );
            user.Clips[index] = newClip;

            var updateUserResponse = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to update user's clips: User: {user.Id} , Clip: {clipId}");

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
            var user = await GetUser(userId);
            var index = user.Clips.FindIndex(x => x.Id == clipId);
            var clip = user.Clips[index];
            user.Clips.RemoveAt(index);

            var updateUserResponse = await _usersContainer.ReplaceItemAsync(user, user.Id, new PartitionKey(user.Id));
            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Unable to remove user's clip: User: {user.Id} , Clip: {clip.Id}");
            serviceResult.Result = clip;
        }
        catch (Exception ex)
        {
            serviceResult.IsError= true;
            serviceResult.ErrorMessage= ex.ToString();
        }
        return serviceResult;
    }

    private async Task<User> GetUser(string userId)
    {
        var userReadResponse = await _usersContainer.ReadItemAsync<User>(userId, new PartitionKey(userId));
        if (userReadResponse.StatusCode != System.Net.HttpStatusCode.OK) 
            throw new Exception($"Unable to find user: {userId}");
        return userReadResponse.Resource;
    }
}
