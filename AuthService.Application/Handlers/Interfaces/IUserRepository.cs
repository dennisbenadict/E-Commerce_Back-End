using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
    Task BlockAsync(int userId);
    Task UnblockAsync(int userId);
    Task<User?> GetByIdAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task MakeAdminAsync(int userId);
    Task RemoveAdminAsync(int userId);
}
