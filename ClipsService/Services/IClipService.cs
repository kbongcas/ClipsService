using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;

namespace ClipsService.Services;
public interface IClipsService
{
    Task<ServiceResult<List<Clip>>> GetClipsOfUser(string userId);
    Task<ServiceResult<Clip>> AddClip(string userId, AddClipRequestDto clip);
    Task<ServiceResult<Clip>> DeleteClip(string userId, string clipId);
    Task<ServiceResult<Clip>> UpdateClip(string userId, string clipId, UpdateClipRequestDto clip);
    Task<ServiceResult<Clip>> UpdateClipUri(string userId, string clipId, UpdateClipUriRequestDto updateClipUriRequestDto);
    Task<ServiceResult<Clip>> GetPublicClip(string userId, string clipId);
}