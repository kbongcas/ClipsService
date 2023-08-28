using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;

namespace ClipsService.Services;
public interface IUserService
{
    Task<ServiceResult<User>> AddUser(AddUserRequestDto createClipRequestDto);
}