using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClipsService.Controllers;
using ClipsService.Models;
using ClipsService.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Text;

namespace ClipsServiceTests.Controllers;
public class ClipsControllerTests
{

    private ClipService _clipService;
    private readonly string _cosmosDbId = "clipdatdb";
    private readonly string _clipsContainerId = "clips";
    private readonly string _connectionString1 = "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";

    private StorageService _storageService;
    private BlobServiceClient _blobServiceClient;
    private readonly string _connectionString2 = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
    private readonly string _convertedClipsContainerName = "convertedclips";

    private ClipsController _clipsController;

    [SetUp]
    public void SetUp()
    {
        var inMemConfig = new Dictionary<string, string>
        {
        };

        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemConfig)
            .Build();

        Environment.SetEnvironmentVariable("ConvertedContainerName", _convertedClipsContainerName);

        _blobServiceClient = new BlobServiceClient(_connectionString2);
        _storageService = new StorageService(config, _blobServiceClient);


        Environment.SetEnvironmentVariable("CosmosDbCosmosDbId", _cosmosDbId);
        Environment.SetEnvironmentVariable("CosmosDbClipsContainerId", _clipsContainerId);
        CosmosClient client = new(_connectionString1);
        _clipService = new ClipService(config, client);

        _clipsController = new ClipsController(_clipService, _storageService);
    }

    [Test]
    public async Task DeleteClipTest()
    {
        CosmosClient client = new(_connectionString1);
        var db = client.GetDatabase(_cosmosDbId);
        var _clipsContainer = db.GetContainer(_clipsContainerId);

        var userId1 = "";
        for (var i = 0; i < 5; i++)
        {
            var newFileName = Guid.NewGuid().ToString();
            var clip1 = new Clip() {
                Id = newFileName,
                Name = "clip" + i,
                Description = "hi",
                UserId = userId1,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now,
                Converted = true,
                Uri = new Uri("https://clipdatsa.blob.core.windows.net/converted/04b44dc1-5ad3-457d-82e1-5c662e479c92.gif")
            };
            await _clipsContainer.CreateItemAsync<Clip>(clip1, new PartitionKey(userId1));

            var html = "<div></div>";
            var gif = "<div></div>";

            BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_convertedClipsContainerName);
            BlobClient blobClient1 = containerClient.GetBlobClient(newFileName + ".html");

            await using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(html)))
            {
                var response = await blobClient1.UploadAsync(ms, new BlobHttpHeaders { ContentType = "text/html" });
            }

            BlobClient blobClient2 = containerClient.GetBlobClient(newFileName + ".gif");
            await using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(gif)))
            {
                var response = await blobClient2.UploadAsync(ms, new BlobHttpHeaders { ContentType = "image/gif" });
            }
        }
    }
}
