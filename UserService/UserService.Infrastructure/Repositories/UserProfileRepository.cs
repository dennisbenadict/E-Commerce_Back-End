using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserServiceDbContext _db;
    public UserProfileRepository(UserServiceDbContext db) => _db = db;

    public async Task<UserProfile?> GetByIdAsync(int id)
        => await _db.UserProfiles.FindAsync(id);

    public async Task AddAsync(UserProfile profile)
    {
        await _db.UserProfiles.AddAsync(profile);
    }

    public async Task UpdateAsync(UserProfile profile)
    {
        _db.UserProfiles.Update(profile);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
