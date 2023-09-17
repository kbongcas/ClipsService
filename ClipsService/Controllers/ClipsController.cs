using ClipsService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClipsService.Controllers;

[ApiController]
[Route("users/")]
[Authorize]
public class ClipsController : ControllerBase
{
    private readonly Services.IClipsService _clipsService;
    private readonly Services.IStorageService _storageService;

    public ClipsController(Services.IClipsService clipsService, Services.IStorageService storageService)
    {
        _clipsService = clipsService;
        _storageService = storageService;
    }

    [HttpGet("my/clips")]
    [Authorize("AbleToReadMyClips")]
    public async Task<IActionResult> GetClips(
        [FromQuery(Name = "page")] int page,
        [FromQuery(Name = "pageSize")] int pageSize
        )
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(userId == null) return Unauthorized();

        if (page < 1) return BadRequest("Page was not valid.");

        var getClipsResultsDto = new GetClipsRequestDto
        {
            PageNumber = page,
            ElementsPerPage = Math.Min(pageSize, 25),
        };

        var serviceResult = await _clipsService.GetClipsOfUser(userId, getClipsResultsDto);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

        var getClipsResponseDto = new GetClipsResponseDto
        {
            Count = serviceResult.Result.Item1,
            Clips = serviceResult.Result.Item2
        };

        return new OkObjectResult(getClipsResponseDto);
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
        try
        {

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var serviceResult = await _clipsService.DeleteClip(userId, id);
            if (serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

            var removeGifServiceResult = await _storageService.RemoveFile(id + ".gif");
            if (removeGifServiceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

            var removeHtmlerviceResult = await _storageService.RemoveFile(id + ".html");
            if (removeHtmlerviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);

            if (!removeGifServiceResult.Result)
            {
                Console.WriteLine($"Attempted to delete {id}.gif, but file is not in storage");
            }
            if (!removeHtmlerviceResult.Result)
            {
                Console.WriteLine($"Attempted to delete {id}.html, but file is not in storage");
            }

            return new OkObjectResult(serviceResult.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPatch("my/clips/{id}")]
    [Authorize("AbleToWriteMyClips")]
    public async Task<IActionResult> UpdateClipAsync(
        string id, 
        [FromBody]UpdateClipRequestDto updateClipRequestDto)
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
            if(userId == null) return new BadRequestResult();

            var serviceResult = await _clipsService.UpdateClipUri(userId, id, updateClipUriRequestDto);
            if (serviceResult.IsError) throw new Exception(serviceResult.ErrorMessage);

            return new OkObjectResult(serviceResult.Result);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message.ToString());
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

    }
}
