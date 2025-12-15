using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services
{
    public class UserProfileService : IUserProfileService
    {
        private readonly IUserProfileRepository _repo;
        private readonly IEventProducer _producer;
        private readonly IPasswordHasher _hasher;
        private readonly ILogger<UserProfileService> _logger;

        public UserProfileService(
            IUserProfileRepository repo,
            IEventProducer producer,
            IPasswordHasher hasher,
            ILogger<UserProfileService> logger)
        {
            _repo = repo;
            _producer = producer;
            _hasher = hasher;
            _logger = logger;
        }

        public async Task<ProfileDto?> GetProfileAsync(int userId)
        {
            var u = await _repo.GetByIdAsync(userId);
            if (u == null) return null;

            // If profile has empty name/email/phone, request sync from AuthService
            if (string.IsNullOrWhiteSpace(u.Name) && string.IsNullOrWhiteSpace(u.Email) && string.IsNullOrWhiteSpace(u.Phone))
            {
                _logger.LogInformation("Profile has empty credentials for UserId {UserId}. Requesting sync from AuthService.", userId);
                
                // Publish sync request event - AuthService will respond with user.data.synced
                // The sync happens asynchronously via RabbitMQ, so user may need to refresh to see updated data
                await _producer.PublishAsync("user.profile.sync.requested", new
                {
                    UserId = userId,
                    Timestamp = DateTime.UtcNow
                });
            }

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
            bool isNew = false;

            if (existing == null)
            {
                isNew = true;
                existing = new UserProfile
                {
                    Id = userId,
                    Name = dto.Name,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    CreatedAt = DateTime.UtcNow
                };

                await _repo.AddAsync(existing);
                _logger.LogInformation("Created new profile for UserId {UserId}", userId);
            }
            else
            {
                existing.Name = dto.Name;
                existing.Email = dto.Email;
                existing.Phone = dto.Phone;

                await _repo.UpdateAsync(existing);
                _logger.LogInformation("Updated profile for UserId {UserId}", userId);
            }

            await _repo.SaveChangesAsync();

            var eventName = isNew ? "user.profile.created" : "user.profile.updated";

            var payload = new
            {
                UserId = existing.Id,
                existing.Name,
                existing.Email,
                existing.Phone,
                existing.CreatedAt,
                Timestamp = DateTime.UtcNow
            };

            await _producer.PublishAsync(eventName, payload);

            _logger.LogInformation("Published event {EventName} for UserId {UserId}", eventName, userId);

            return new ProfileDto
            {
                Id = existing.Id,
                Name = existing.Name,
                Email = existing.Email,
                Phone = existing.Phone,
                CreatedAt = existing.CreatedAt
            };
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return false;

            // If password hash is missing (e.g., user created before sync was added),
            // we allow password change but can't verify old password
            // User is already authenticated via JWT, so this is acceptable
            if (string.IsNullOrEmpty(user.PasswordHash))
            {
                _logger.LogWarning(
                    "Password hash missing for UserId {UserId}. Allowing password change without old password verification (user is authenticated via JWT).",
                    userId
                );
                // Proceed to set new password without verification
            }
            else
            {
                // Verify old password if hash exists
                if (!_hasher.Verify(user.PasswordHash, dto.CurrentPassword))
                {
                    _logger.LogWarning("Password change failed for UserId {UserId}: wrong old password", userId);
                    return false;
                }
            }

            // Hash and save new password
            user.PasswordHash = _hasher.Hash(dto.NewPassword);

            await _repo.UpdateAsync(user);
            await _repo.SaveChangesAsync();

            // Sync new password hash to AuthService
            await _producer.PublishAsync("user.password.changed", new
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Password changed successfully for UserId {UserId}", userId);
            _logger.LogInformation("Published event user.password.changed for UserId {UserId}", userId);

            return true;
        }
    }
}

