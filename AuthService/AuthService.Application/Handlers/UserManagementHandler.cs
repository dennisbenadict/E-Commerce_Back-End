using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Handlers;

public class UserManagementHandler
{
    private readonly IUserRepository _repo;

    public UserManagementHandler(IUserRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task BlockUserAsync(int userId)
    {
        await _repo.BlockAsync(userId);
    }

    public async Task UnblockUserAsync(int userId)
    {
        await _repo.UnblockAsync(userId);
    }

    public async Task MakeAdminAsync(int userId)
    {
        await _repo.MakeAdminAsync(userId);
    }

    public async Task RemoveAdminAsync(int userId)
    {
        await _repo.RemoveAdminAsync(userId);
    }
}

