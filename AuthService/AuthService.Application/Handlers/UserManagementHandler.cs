using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Application.Handlers;

public class UserManagementHandler
{
    private readonly IUserRepository _repo;
    private readonly IEventProducer _producer;

    public UserManagementHandler(IUserRepository repo, IEventProducer producer)
    {
        _repo = repo;
        _producer = producer;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task BlockUserAsync(int userId)
    {
        await _repo.BlockAsync(userId);
        await _producer.PublishAsync("user.blocked", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task UnblockUserAsync(int userId)
    {
        await _repo.UnblockAsync(userId);
        await _producer.PublishAsync("user.unblocked", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task MakeAdminAsync(int userId)
    {
        await _repo.MakeAdminAsync(userId);
        await _producer.PublishAsync("user.admin.made", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task RemoveAdminAsync(int userId)
    {
        await _repo.RemoveAdminAsync(userId);
        await _producer.PublishAsync("user.admin.removed", new
        {
            UserId = userId,
            Timestamp = DateTime.UtcNow
        });
    }
}

