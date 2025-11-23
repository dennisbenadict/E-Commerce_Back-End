using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using System.Threading.Tasks;

namespace AuthService.Application.Handlers;

public class LoginHandler
{
    private readonly IUserRepository _repo;

    public LoginHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _repo.GetByEmailAsync(email);
    }
}



