using ClipsService.Dtos;
using ClipsService.Models;
using ClipsService.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace ClipsServiceTests.Services;
public class ClipServiceTest
{

    private ClipService _clipService;
    private readonly string _cosmosDbId = "clipdatdb";
    private readonly string _clipsContainerId = "clips";
    private readonly string _connectionString = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    [SetUp]
    public void SetUp()
    {
        var inMemConfig = new Dictionary<string, string>
        {
            { "CosmosDbCosmosDbId", _cosmosDbId },
            { "CosmosDbClipsContainerId", _clipsContainerId }
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        // New instance of CosmosClient class using an endpoint and key string
        CosmosClient client = new(_connectionString);

        _clipService = new ClipService(config, client);
    }

    [Test]
    public async Task TestAddClip()
    {
        var userId = "userId";
        var addRequestDto = new AddClipRequestDto()
        {
            Name = "Name1",
            Description = "Description",
            Uri = "https://www.fakeuri.com",
            Converted = true,
            Public = true
        };

        var result = await _clipService.AddClip(userId, addRequestDto);
        Assert.IsFalse(result.IsError);
        var createdClip = result.Result;

        CosmosClient client = new(_connectionString);
        var db = client.GetDatabase(_cosmosDbId);
        var _clipsContainer = db.GetContainer(_clipsContainerId);

        var response = await  _clipsContainer.ReadItemAsync<Clip>(createdClip.Id, new PartitionKey(userId));
        Assert.IsNotNull(response.Resource);

    }

    [Test]
    public async Task GetClipsOfUserTest()
    {

        CosmosClient client = new(_connectionString);
        var db = client.GetDatabase(_cosmosDbId);
        var _clipsContainer = db.GetContainer(_clipsContainerId);

        var userId1 = "userId1";
        var clip1 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId1 };
        var clip2 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId1};

        var userId2 = "userId2";
        var clip3 = new Clip() {  Id = Guid.NewGuid().ToString(), UserId = userId2};
        var creatNewClipResponse1 = await _clipsContainer.CreateItemAsync(clip1, new PartitionKey(userId1));
        var creatNewClipResponse2 = await _clipsContainer.CreateItemAsync(clip2, new PartitionKey(userId1));
        var creatNewClipResponse3 = await _clipsContainer.CreateItemAsync(clip3, new PartitionKey(userId2));

        var userId3 = "userId3";

        var result = await _clipService.GetClipsOfUser(userId1);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Count == 2);

        result = await _clipService.GetClipsOfUser(userId2);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Count == 1);

        var createdClip = result.Result;
        result = await _clipService.GetClipsOfUser(userId3);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Count == 0);
    }


    [Test]
    public async Task DeleteClipTest()
    {

        CosmosClient client = new(_connectionString);
        var db = client.GetDatabase(_cosmosDbId);
        var _clipsContainer = db.GetContainer(_clipsContainerId);

        var userId1 = "userId1231";
        var clip1 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId1 };
        var clip2 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId1};

        var creatNewClipResponse1 = await _clipsContainer.CreateItemAsync(clip1, new PartitionKey(userId1));
        var creatNewClipResponse2 = await _clipsContainer.CreateItemAsync(clip2, new PartitionKey(userId1));


        var serviceResult = await _clipService.DeleteClip(userId1, clip1.Id);
        Assert.IsTrue(serviceResult.IsError);

        var queryDefinition = new QueryDefinition($"SELECT * FROM {_clipsContainerId}");
        var iterator = _clipsContainer.GetItemQueryIterator<Clip>(
            queryDefinition,
            requestOptions: new QueryRequestOptions()
            {
                PartitionKey = new PartitionKey(userId1)
            });
        var results = new List<Clip>();
        while (iterator.HasMoreResults)
        {
            var result = await iterator.ReadNextAsync();
            results.AddRange(result.Resource);
        }

        Assert.IsTrue(results.Count == 1);

    }

}
