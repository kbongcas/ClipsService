
using ClipsService.Errors;

namespace ClipsService.Services;

public interface IStorageService
{
    public Task<ServiceResult<bool>> RemoveFile(string fileName);
}