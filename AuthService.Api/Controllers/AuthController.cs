using AuthService.Application.DTOs;
using AuthService.Application.Handlers;
using AuthService.Application.Services;
using AuthService.Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RegisterHandler _register;
    private readonly LoginHandler _login;
    private readonly IJwtTokenService _tokenService;

    public AuthController(
        RegisterHandler register,
        LoginHandler login,
        IJwtTokenService tokenService)
    {
        _register = register;
        _login = login;
        _tokenService = tokenService;
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
        var hash = PasswordHasher.Hash(dto.Password);
        var user = await _login.ValidateAsync(dto.Email, hash);

        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        var token = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);
        return Ok(new { token });
    }
}
