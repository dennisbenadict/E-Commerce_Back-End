using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Handlers;

public class LoginHandler
{
    private readonly IUserRepository _repo;

    public LoginHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<User?> ValidateAsync(string email, string passwordHash)
    {
        var user = await _repo.GetByEmailAsync(email);
        if (user == null) return null;

        return user.PasswordHash == passwordHash ? user : null;
    }
}

