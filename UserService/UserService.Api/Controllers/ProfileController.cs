using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users/profile")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserProfileService _service;
    public ProfileController(IUserProfileService service) => _service = service;

    private int GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(v)) throw new Exception("User id missing in token");
        return int.Parse(v);
    }


    [HttpGet]
    [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get()
    {
        var id = GetUserId();
        var profile = await _service.GetProfileAsync(id);

        if (profile == null)
            return NotFound(new { message = "Profile not found" });

        return Ok(profile);
    }


    [HttpPut]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update([FromBody] UpdateProfileDto dto)
    {
        var id = GetUserId();
        var updated = await _service.UpdateProfileAsync(id, dto);

        if (updated == null)
            return BadRequest(new { message = "Profile update failed" });

        return Ok(new { message = "Profile updated successfully", profile = updated });
    }

    [HttpPut("change-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
    {
        var userId = GetUserId();

        var result = await _service.ChangePasswordAsync(userId, dto);

        if (!result)
            return BadRequest(new { message = "Current password is incorrect" });

        return Ok(new { message = "Password changed successfully" });
    }

}

