using AuthService.Application.DTOs;
using AuthService.Application.Handlers;
using AuthService.Application.Services;
using AuthService.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterHandler _register;
    private readonly LoginHandler _login;
    private readonly UserManagementHandler _users;
    private readonly IJwtTokenService _tokenService;

    public AuthController(
        RegisterHandler register,
        LoginHandler login,
        IJwtTokenService tokenService,
        UserManagementHandler users)
    {
        _register = register;
        _login = login;
        _tokenService = tokenService;
        _users = users;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var hash = PasswordHasher.Hash(dto.Password);

        await _register.ExecuteAsync(dto.Name, dto.Phone, dto.Email, hash);
        return Ok(new { message = "Registered successfully" });
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _login.GetUserByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        bool isPasswordValid = PasswordHasher.Verify(dto.Password, user.PasswordHash);
        if (!isPasswordValid)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);
        return Ok(new { token });
    }


    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            Email = User.FindFirst(ClaimTypes.Email)?.Value,
            Role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }


    [Authorize(Roles = "Admin")]
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _users.GetAllUsersAsync());
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("block/{userId}")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        await _users.BlockUserAsync(userId);
        return Ok(new { message = "User blocked" });
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("unblock/{userId}")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        await _users.UnblockUserAsync(userId);
        return Ok(new { message = "User unblocked" });
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("make-admin/{userId}")]
    public async Task<IActionResult> MakeAdmin(int userId)
    {
        await _users.MakeAdminAsync(userId);
        return Ok(new { message = "User promoted to Admin" });
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("remove-admin/{userId}")]
    public async Task<IActionResult> RemoveAdmin(int userId)
    {
        await _users.RemoveAdminAsync(userId);
        return Ok(new { message = "Admin role removed" });
    }
}

