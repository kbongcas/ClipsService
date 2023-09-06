using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using ClipsService.Services;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Text;

namespace ClipsServiceTests.Services;
public class StorageServiceTest
{
    private StorageService _storageService;
    private BlobServiceClient _blobServiceClient;
    private readonly string _connectionString = "AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;DefaultEndpointsProtocol=http;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;";
    private readonly string _convertedClipsContainerName = "convertedclips";

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

        _blobServiceClient = new BlobServiceClient(_connectionString);
        _storageService = new StorageService(config, _blobServiceClient);

    }

    [Test]
    public async Task RemoveFileTest()
    {
        var newFileName = Guid.NewGuid().ToString();
        var html = "<div></div>";

        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_convertedClipsContainerName);
        BlobClient blobClient = containerClient.GetBlobClient(newFileName + ".html");
        if (blobClient.Exists()) await blobClient.DeleteAsync();

        await using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(html)))
        {
            var response = await blobClient.UploadAsync(ms, new BlobHttpHeaders { ContentType = "text/html" });
        }
        var resp = await _storageService.RemoveFile(newFileName + ".html");
        Assert.False(resp.IsError);

        blobClient = containerClient.GetBlobClient(newFileName + ".html");
        Assert.That( (await blobClient.ExistsAsync()).Value, Is.False);
    }

}
