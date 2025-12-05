namespace AuthService.Application.Services;

public interface IJwtTokenService
{
    string GenerateToken(int userId, string email, bool isAdmin);
}
