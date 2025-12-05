using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Domain.Utils;


namespace AuthService.Application.Services;

public class RefreshTokenService
{
    private readonly IUserRepository _repo;

    public RefreshTokenService(IUserRepository repo)
    {
        _repo = repo;
    }


    public async Task<(string rawToken, RefreshToken tokenEntity)> CreateRefreshTokenAsync(int userId)
    {
        var raw = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        var hash = PasswordHasher.Hash(raw);

        var entity = new RefreshToken
        {
            UserId = userId,
            Token = hash,  // stored as hash in DB
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddRefreshTokenAsync(entity);
        await _repo.SaveChangesAsync();

        return (raw, entity);
    }


    public async Task<RefreshToken?> GetByRawTokenAsync(string rawToken)
    {
        var hash = PasswordHasher.Hash(rawToken);
        return await _repo.GetRefreshTokenAsync(hash);
    }


    public async Task RevokeAsync(RefreshToken token)
    {
        await _repo.RevokeRefreshTokenAsync(token);
    }


    public async Task RevokeAllForUserAsync(int userId)
    {
        await _repo.RevokeAllRefreshTokensForUserAsync(userId);
    }
}

