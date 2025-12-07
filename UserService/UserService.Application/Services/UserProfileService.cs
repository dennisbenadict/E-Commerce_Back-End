using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repo;
    private readonly IUserProfileRepository _profileRepo; // same
    public UserProfileService(IUserProfileRepository repo)
    {
        _repo = repo;
        _profileRepo = repo;
    }

    public async Task<ProfileDto?> GetProfileAsync(int userId)
    {
        var u = await _repo.GetByIdAsync(userId);
        if (u == null) return null;
        return new ProfileDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Phone = u.Phone,
            CreatedAt = u.CreatedAt
        };
    }

    public async Task<ProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var existing = await _repo.GetByIdAsync(userId);
        if (existing == null)
        {
            existing = new UserProfile
            {
                Id = userId,
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(existing);
        }
        else
        {
            existing.Name = dto.Name;
            existing.Phone = dto.Phone;
            existing.Email = dto.Email;
            await _repo.UpdateAsync(existing);
        }

        await _repo.SaveChangesAsync();

        return new ProfileDto
        {
            Id = existing.Id,
            Name = existing.Name,
            Email = existing.Email,
            Phone = existing.Phone,
            CreatedAt = existing.CreatedAt
        };
    }
}
