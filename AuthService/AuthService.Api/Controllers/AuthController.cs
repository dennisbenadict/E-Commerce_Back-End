//using AuthService.Application.DTOs;
//using AuthService.Application.Handlers;
//using AuthService.Application.Services;
//using AuthService.Application.Interfaces;
//using AuthService.Domain.Utils;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Authorization;
//using System.Security.Claims;

//namespace AuthService.Api.Controllers;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthController : ControllerBase
//{
//    private readonly RegisterHandler _register;
//    private readonly LoginHandler _login;
//    private readonly UserManagementHandler _users;
//    private readonly IJwtTokenService _tokenService;
//    private readonly RefreshTokenService _refreshService;
//    private readonly IUserRepository _userRepo;

//    public AuthController(
//        RegisterHandler register,
//        LoginHandler login,
//        IJwtTokenService tokenService,
//        UserManagementHandler users,
//        RefreshTokenService refreshService,
//        IUserRepository userRepo)
//    {
//        _register = register;
//        _login = login;
//        _tokenService = tokenService;
//        _users = users;
//        _refreshService = refreshService;
//        _userRepo = userRepo;
//    }


//    [HttpPost("register")]
//    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
//    {
//        var hash = PasswordHasher.Hash(dto.Password);

//        await _register.ExecuteAsync(
//            dto.Name.Trim(),
//            dto.Phone.Trim(),
//            dto.Email.Trim().ToLowerInvariant(),
//            hash
//        );

//        return Ok(new { message = "Registered successfully" });
//    }


//    [HttpPost("login")]
//    public async Task<IActionResult> Login([FromBody] LoginDto dto)
//    {
//        var user = await _login.GetUserByEmailAsync(dto.Email.Trim().ToLowerInvariant());
//        if (user == null)
//            return Unauthorized(new { message = "Invalid credentials" });

//        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
//            return Unauthorized(new { message = "Invalid credentials" });

//        if (user.IsBlocked)
//            return Forbid();

//        var accessToken = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);

//        var (rawRefresh, refreshEntity) = await _refreshService.CreateRefreshTokenAsync(user.Id);

//        bool isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

//        Response.Cookies.Append("refresh_token", rawRefresh, new CookieOptions
//        {
//            HttpOnly = true,
//            Secure = true,
//            SameSite = SameSiteMode.None,
//            Expires = refreshEntity.ExpiresAt,
//            Path = "/"
//        });

//        return Ok(new { accessToken });
//    }


//    [HttpPost("refresh")]
//    public async Task<IActionResult> Refresh()
//    {
//        var raw = Request.Cookies["refresh_token"];
//        if (string.IsNullOrEmpty(raw))
//            return Unauthorized(new { message = "No refresh token provided" });

//        var existing = await _refreshService.GetByRawTokenAsync(raw);

//        if (existing == null || !existing.IsActive)
//            return Unauthorized(new { message = "Invalid or expired refresh token" });

//        await _refreshService.RevokeAsync(existing);

//        var (newRaw, newEntity) = await _refreshService.CreateRefreshTokenAsync(existing.UserId);

//        var user = await _userRepo.GetByIdAsync(existing.UserId);
//        if (user == null || user.IsBlocked)
//            return Unauthorized(new { message = "User not found or blocked" });

//        var newAccess = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);

//        bool isDev = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

//        Response.Cookies.Append("refresh_token", newRaw, new CookieOptions
//        {
//            HttpOnly = true,
//            Secure = true,
//            SameSite = SameSiteMode.None,
//            Expires = newEntity.ExpiresAt,
//            Path = "/"
//        });

//        return Ok(new { accessToken = newAccess });
//    }


//    [HttpPost("logout")]
//    public async Task<IActionResult> Logout()
//    {
//        var raw = Request.Cookies["refresh_token"];

//        if (!string.IsNullOrEmpty(raw))
//        {
//            var existing = await _refreshService.GetByRawTokenAsync(raw);

//            if (existing != null && existing.IsActive)
//                await _refreshService.RevokeAsync(existing);

//            Response.Cookies.Append("refresh_token", "", new CookieOptions
//            {
//                HttpOnly = true,
//                Secure = false,
//                Expires = DateTime.UtcNow.AddDays(-1),
//                Path = "/api/auth/refresh"
//            });
//        }

//        return Ok(new { message = "Logged out" });
//    }


//    [Authorize]
//    [HttpGet("me")]
//    public IActionResult Me()
//    {
//        return Ok(new
//        {
//            Id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
//            Email = User.FindFirst(ClaimTypes.Email)?.Value,
//            Role = User.FindFirst(ClaimTypes.Role)?.Value
//        });
//    }


//    [Authorize(Roles = "Admin")]
//    [HttpGet("users")]
//    public async Task<IActionResult> GetAllUsers()
//    {
//        return Ok(await _users.GetAllUsersAsync());
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpPost("block/{userId}")]
//    public async Task<IActionResult> BlockUser(int userId)
//    {
//        await _users.BlockUserAsync(userId);
//        await _refreshService.RevokeAllForUserAsync(userId); // block tokens
//        return Ok(new { message = "User blocked" });
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpPost("unblock/{userId}")]
//    public async Task<IActionResult> UnblockUser(int userId)
//    {
//        await _users.UnblockUserAsync(userId);
//        return Ok(new { message = "User unblocked" });
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpPost("make-admin/{userId}")]
//    public async Task<IActionResult> MakeAdmin(int userId)
//    {
//        await _users.MakeAdminAsync(userId);
//        return Ok(new { message = "User promoted to Admin" });
//    }

//    [Authorize(Roles = "Admin")]
//    [HttpPost("remove-admin/{userId}")]
//    public async Task<IActionResult> RemoveAdmin(int userId)
//    {
//        await _users.RemoveAdminAsync(userId);
//        return Ok(new { message = "Admin role removed" });
//    }
//}



using AuthService.Application.DTOs;
using AuthService.Application.Handlers;
using AuthService.Application.Services;
using AuthService.Application.Interfaces;
using AuthService.Domain.Utils;
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
    private readonly RefreshTokenService _refreshService;
    private readonly IUserRepository _userRepo;

    public AuthController(
        RegisterHandler register,
        LoginHandler login,
        IJwtTokenService tokenService,
        UserManagementHandler users,
        RefreshTokenService refreshService,
        IUserRepository userRepo)
    {
        _register = register;
        _login = login;
        _tokenService = tokenService;
        _users = users;
        _refreshService = refreshService;
        _userRepo = userRepo;
    }


    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var hash = PasswordHasher.Hash(dto.Password);

        await _register.ExecuteAsync(
            dto.Name.Trim(),
            dto.Phone.Trim(),
            dto.Email.Trim().ToLowerInvariant(),
            hash
        );

        return Ok(new { message = "Registered successfully" });
    }


    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _login.GetUserByEmailAsync(dto.Email.Trim().ToLowerInvariant());
        if (user == null)
            return Unauthorized(new { message = "Invalid credentials" });

        if (!PasswordHasher.Verify(dto.Password, user.PasswordHash))
            return Unauthorized(new { message = "Invalid credentials" });

        if (user.IsBlocked)
            return Forbid("User is blocked");

        var accessToken = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);

        var (rawRefresh, refreshEntity) = await _refreshService.CreateRefreshTokenAsync(user.Id);

        Response.Cookies.Append("refresh_token", rawRefresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = refreshEntity.ExpiresAt,
            Path = "/"
        });

        return Ok(new
        {
            message = "Login successful",
            accessToken
        });
    }


    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh()
    {
        var raw = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(raw))
            return Unauthorized(new { message = "No refresh token provided" });

        var existing = await _refreshService.GetByRawTokenAsync(raw);
        if (existing == null || !existing.IsActive)
            return Unauthorized(new { message = "Invalid or expired refresh token" });

        await _refreshService.RevokeAsync(existing);

        var (newRaw, newEntity) = await _refreshService.CreateRefreshTokenAsync(existing.UserId);

        var user = await _userRepo.GetByIdAsync(existing.UserId);
        if (user == null || user.IsBlocked)
            return Unauthorized(new { message = "User not found or blocked" });

        var newAccess = _tokenService.GenerateToken(user.Id, user.Email, user.IsAdmin);

        Response.Cookies.Append("refresh_token", newRaw, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = newEntity.ExpiresAt,
            Path = "/"
        });

        return Ok(new
        {
            message = "Token refreshed",
            accessToken = newAccess
        });
    }


    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Logout()
    {
        var raw = Request.Cookies["refresh_token"];

        if (!string.IsNullOrEmpty(raw))
        {
            var existing = await _refreshService.GetByRawTokenAsync(raw);

            if (existing != null && existing.IsActive)
                await _refreshService.RevokeAsync(existing);

            Response.Cookies.Append("refresh_token", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                Expires = DateTime.UtcNow.AddDays(-1),
                Path = "/api/auth/refresh"
            });
        }

        return Ok(new { message = "Logged out successfully" });
    }


    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers()
    {
        return Ok(await _users.GetAllUsersAsync());
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("block/{userId}")]
    public async Task<IActionResult> BlockUser(int userId)
    {
        await _users.BlockUserAsync(userId);
        await _refreshService.RevokeAllForUserAsync(userId);

        return Ok(new { message = "User blocked successfully" });
    }


    [Authorize(Roles = "Admin")]
    [HttpPost("unblock/{userId}")]
    public async Task<IActionResult> UnblockUser(int userId)
    {
        await _users.UnblockUserAsync(userId);
        return Ok(new { message = "User unblocked successfully" });
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
        return Ok(new { message = "Admin role removed from user" });
    }
}

