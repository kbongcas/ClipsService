using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Diagnostics;
using User = ClipsService.Models.User;

namespace ClipsService.Services;
public class UserService : IUserService
{
    private readonly Container _usersContainer;

    public UserService(IConfiguration configuration, CosmosClient cosmosClient)
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

    public async Task<ServiceResult<User>> AddUser(AddUserRequestDto createClipRequestDto)
    {
        ServiceResult<User> serviceResult = new();
        try
        {
            var user = new User()
            {
                Id = createClipRequestDto.Id,
                Name = "",
                Clips = new List<Clip>()
            };

            var updateUserResponse = await _usersContainer.CreateItemAsync(user, new PartitionKey(user.Id));

            if (updateUserResponse.StatusCode != System.Net.HttpStatusCode.Created)
                throw new Exception($"Unable to Create User: {user.Id}");

            serviceResult.Result = user;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.Message;
        }

        return serviceResult;
    }

}
