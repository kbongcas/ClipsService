using ClipsService.Dtos;
using ClipsService.Errors;
using ClipsService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration.UserSecrets;
using Newtonsoft.Json;
using System.Data.Common;
using System.Security.Claims;

namespace ClipsService.Controllers;

[ApiController]
[Route("users/")]
[Authorize]
public class ClipsController : ControllerBase
{
    private readonly Services.IClipsService _clipsService;

    public ClipsController(Services.IClipsService clipsService)
    {
        _clipsService = clipsService;
    }

    [HttpGet("my/clips")]
    [Authorize("AbleToReadMyClips")]
    public async Task<IActionResult> GetClips()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();

        var serviceResult = await _clipsService.GetClips(userId);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

        return new OkObjectResult(serviceResult.Result);
    }

    [HttpPost("{userId}/clips")]
    [Authorize("AbleToWriteOtherUserClips")]
    public async Task<IActionResult> AddClip([FromBody]AddClipRequestDto addClipRequestDto, string userId)
    {
        if(userId == null) return StatusCode(StatusCodes.Status500InternalServerError);

        var serviceResult = await _clipsService.AddClip(userId, addClipRequestDto);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

        return new OkObjectResult(serviceResult.Result);
    }

    [HttpDelete("my/clips/{id}")]
    [Authorize("AbleToWriteMyClips")]
    public async Task<IActionResult> DeleteClip(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();

        var serviceResult = await _clipsService.DeleteClip(userId, id);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);
        return new OkObjectResult(serviceResult.Result);
    }

    [HttpPatch("my/clips/{id}")]
    [Authorize("AbleToWriteMyClips")]
    public async Task<IActionResult> UpdateClipAsync(
        string id, 
        [FromBody]UpdateMyClipRequestDto updateClipRequestDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();

        var serviceResult = await _clipsService.UpdateClip(userId, id, updateClipRequestDto);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

        return new OkObjectResult(serviceResult.Result);
    }

    [HttpPatch("{userId}/clips/{id}")]
    [Authorize("AbleToWriteOtherUserClips")]
    public async Task<IActionResult> UpdateUsersClipAsync(
        string userId,
        string id, 
        [FromBody]UpdateClipRequestDto updateClipRequestDto)
    {
        if(userId == null) return new BadRequestResult();

        var serviceResult = await _clipsService.UpdateClip(userId, id, updateClipRequestDto);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

        return new OkObjectResult(serviceResult.Result);
    }

    [HttpPatch("{userId}/clips/{id}/uri")]
    [Authorize("AbleToWriteOtherUserClips")]
    public async Task<IActionResult> UpdateUsersClipUriAsync(
        string userId,
        string id, 
        [FromBody]UpdateClipUriRequestDto updateClipUriRequestDto)
    {
        try
        {
            Console.WriteLine($"UpdateUsersClipUri Called: {JsonConvert.SerializeObject(updateClipUriRequestDto)}");
            if(userId == null) return new BadRequestResult();
            Console.WriteLine($"UpdateUsersClipUri Called UserId: {userId}");
            Console.WriteLine($"UpdateUsersClipUri Called UserId: {id}");

            var serviceResult = await _clipsService.UpdateClipUri(userId, id, updateClipUriRequestDto);
            if (serviceResult.IsError) throw new Exception(serviceResult.ErrorMessage);

            Console.WriteLine($"UpdateUsersClipUri Responsed: {JsonConvert.SerializeObject(serviceResult.Result)}");
            return new OkObjectResult(serviceResult.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }

}
