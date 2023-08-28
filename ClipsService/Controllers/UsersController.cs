using ClipsService.Dtos;
using ClipsService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

[ApiController]
[Route("/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    [Authorize("AbleToWriteUsers")]
    public async Task<IActionResult> AddUser([FromBody]AddUserRequestDto addClipRequestDto)
    {
        var serviceResult = await _userService.AddUser(addClipRequestDto);
        if(serviceResult.IsError) return StatusCode(StatusCodes.Status500InternalServerError);
        return new OkObjectResult(serviceResult.Result);
    }
}
