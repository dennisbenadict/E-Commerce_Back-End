using AuthService.Domain.Entities;

namespace AuthService.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(int id);
    Task AddAsync(User user);
    Task SaveChangesAsync();

    Task<IEnumerable<User>> GetAllAsync();
    Task BlockAsync(int userId);
    Task UnblockAsync(int userId);
    Task MakeAdminAsync(int userId);
    Task RemoveAdminAsync(int userId);


    Task AddRefreshTokenAsync(RefreshToken token);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash);
    Task RevokeRefreshTokenAsync(RefreshToken token);
    Task RevokeAllRefreshTokensForUserAsync(int userId);
}
