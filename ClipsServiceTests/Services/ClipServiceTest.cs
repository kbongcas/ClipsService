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
        };

        Environment.SetEnvironmentVariable("CosmosDbCosmosDbId", _cosmosDbId);
        Environment.SetEnvironmentVariable("CosmosDbClipsContainerId", _clipsContainerId);


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

        var userId1 = "";
        var userId2 = "userId2";
        for (var i = 0; i < 30; i++)
        {
            var clip1 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId1 };
            await _clipsContainer.CreateItemAsync<Clip>(clip1, new PartitionKey(userId1));
        }

        for (var i = 0; i < 5; i++)
        {
            var clip2 = new Clip() { Id = Guid.NewGuid().ToString(), UserId = userId2 };
            await _clipsContainer.CreateItemAsync<Clip>(clip2, new PartitionKey(userId2));
        }

      
        var getClipsRequestDto = new GetClipsRequestDto()
        {
            PageNumber = 2,
            ElementsPerPage = 10,
        };

        var result = await _clipService.GetClipsOfUser(userId1, getClipsRequestDto);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Item1 == 30);
        Assert.IsTrue(result.Result.Item2.Count == 10);

        getClipsRequestDto = new GetClipsRequestDto()
        {
            PageNumber = 4,
            ElementsPerPage = 8,
        };

        result = await _clipService.GetClipsOfUser(userId1, getClipsRequestDto);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Item1 == 30);
        Assert.IsTrue(result.Result.Item2.Count == 6);


        getClipsRequestDto = new GetClipsRequestDto()
        {
            PageNumber = -1,
            ElementsPerPage = 8,
        };

        result = await _clipService.GetClipsOfUser(userId1, getClipsRequestDto);
        Assert.IsFalse(result.IsError);
        Assert.IsTrue(result.Result.Item1 == 30);
        Assert.That(result.Result.Item2.Count, Is.EqualTo(8));

        getClipsRequestDto = new GetClipsRequestDto()
        {
            PageNumber = 1,
            ElementsPerPage = 2,
        };

        result = await _clipService.GetClipsOfUser(userId2, getClipsRequestDto);
        Assert.IsFalse(result.IsError);
        Assert.That(result.Result.Item1, Is.EqualTo(5));
        Assert.That(result.Result.Item2.Count, Is.EqualTo(2));


        getClipsRequestDto = new GetClipsRequestDto()
        {
            PageNumber = 50,
            ElementsPerPage = 2,
        };

        result = await _clipService.GetClipsOfUser(userId2, getClipsRequestDto);
        Assert.IsFalse(result.IsError);
        Assert.That(result.Result.Item1, Is.EqualTo(5));
        Assert.That(result.Result.Item2.Count, Is.EqualTo(0));

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
