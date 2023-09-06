using Azure.Storage.Blobs;
using ClipsService.Errors;

namespace ClipsService.Services;
public class StorageService : IStorageService
{

    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _convertedContainerClient;

    public StorageService(
        IConfiguration config,
        BlobServiceClient blobServiceClient
        )
    {
        _blobServiceClient = blobServiceClient;

        _convertedContainerClient = _blobServiceClient.GetBlobContainerClient(
            Environment.GetEnvironmentVariable("ConvertedContainerName"));
    }

    public async Task<ServiceResult<bool>> RemoveFile(string fileName)
    {
        ServiceResult<bool> serviceResult = new();
        try
        {
            BlobClient blobClient = _convertedContainerClient.GetBlobClient(fileName);
            var response = await blobClient.DeleteIfExistsAsync();
            serviceResult.Result = response.Value;
        }
        catch (Exception ex)
        {
            serviceResult.IsError = true;
            serviceResult.ErrorMessage = ex.ToString();
        }
        return serviceResult; ;
    }

}
