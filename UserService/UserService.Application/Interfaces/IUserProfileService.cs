using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IUserProfileService
{
    Task<ProfileDto?> GetProfileAsync(int userId);
    Task<ProfileDto?> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}
