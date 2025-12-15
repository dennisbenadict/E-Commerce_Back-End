using Microsoft.Extensions.Logging;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;
        private readonly IUserProfileRepository _profileRepo;
        private readonly IEventProducer _producer;
        private readonly ILogger<AddressService> _logger;

        public AddressService(
            IAddressRepository repo,
            IUserProfileRepository profileRepo,
            IEventProducer producer,
            ILogger<AddressService> logger)
        {
            _repo = repo;
            _profileRepo = profileRepo;
            _producer = producer;
            _logger = logger;
        }

        public async Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId)
        {
            var list = await _repo.GetByUserIdAsync(userId);

            _logger.LogInformation("Fetched {Count} addresses for UserId {UserId}", list.Count(), userId);

            return list.Select(a => new AddressDto
            {
                Id = a.Id,
                UserId = a.UserId,
                FullName = a.FullName,
                Street = a.Street,
                City = a.City,
                State = a.State,
                ZipCode = a.ZipCode,
                Country = a.Country,
                Phone = a.Phone
            }).ToList();
        }

        public async Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto)
        {
            // Ensure UserProfile exists (required for foreign key constraint)
            var profile = await _profileRepo.GetByIdAsync(userId);
            if (profile == null)
            {
                // Create a minimal profile if it doesn't exist
                // This can happen if user was created before RabbitMQ sync was working
                _logger.LogWarning("UserProfile not found for UserId {UserId}. Creating minimal profile.", userId);
                profile = new UserProfile
                {
                    Id = userId,
                    Name = "", // Will be updated when profile is synced
                    Email = "",
                    Phone = "",
                    CreatedAt = DateTime.UtcNow
                };
                await _profileRepo.AddAsync(profile);
                await _profileRepo.SaveChangesAsync();
                _logger.LogInformation("Created minimal UserProfile for UserId {UserId}", userId);
            }
            
            var ent = new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                Street = dto.Street,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode,
                Country = dto.Country,
                Phone = dto.Phone
            };

            await _repo.AddAsync(ent);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Created address {AddressId} for UserId {UserId}", ent.Id, userId);

            await _producer.PublishAsync("user.address.created", new
            {
                AddressId = ent.Id,
                ent.UserId,
                ent.FullName,
                ent.Street,
                ent.City,
                ent.State,
                ent.ZipCode,
                ent.Country,
                ent.Phone,
                Timestamp = DateTime.UtcNow
            });

            return new AddressDto
            {
                Id = ent.Id,
                UserId = ent.UserId,
                FullName = ent.FullName,
                Street = ent.Street,
                City = ent.City,
                State = ent.State,
                ZipCode = ent.ZipCode,
                Country = ent.Country,
                Phone = ent.Phone
            };
        }

        public async Task<AddressDto?> UpdateAddressAsync(int userId, int addressId, CreateAddressDto dto)
        {
            var ent = await _repo.GetByIdAsync(addressId);
            if (ent == null || ent.UserId != userId)
            {
                _logger.LogWarning("Unauthorized update attempt on AddressId {AddressId} by UserId {UserId}", addressId, userId);
                return null;
            }

            ent.FullName = dto.FullName;
            ent.Street = dto.Street;
            ent.City = dto.City;
            ent.State = dto.State;
            ent.ZipCode = dto.ZipCode;
            ent.Country = dto.Country;
            ent.Phone = dto.Phone;

            await _repo.UpdateAsync(ent);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Updated address {AddressId} for UserId {UserId}", addressId, userId);

            await _producer.PublishAsync("user.address.updated", new
            {
                AddressId = ent.Id,
                ent.UserId,
                ent.FullName,
                ent.Street,
                ent.City,
                ent.State,
                ent.ZipCode,
                ent.Country,
                ent.Phone,
                Timestamp = DateTime.UtcNow
            });

            return new AddressDto
            {
                Id = ent.Id,
                UserId = ent.UserId,
                FullName = ent.FullName,
                Street = ent.Street,
                City = ent.City,
                State = ent.State,
                ZipCode = ent.ZipCode,
                Country = ent.Country,
                Phone = ent.Phone
            };
        }

        public async Task<bool> DeleteAddressAsync(int userId, int addressId)
        {
            var ent = await _repo.GetByIdAsync(addressId);
            if (ent == null || ent.UserId != userId)
            {
                _logger.LogWarning("Unauthorized delete attempt on AddressId {AddressId} by UserId {UserId}", addressId, userId);
                return false;
            }

            await _repo.DeleteAsync(ent);
            await _repo.SaveChangesAsync();

            _logger.LogInformation("Deleted address {AddressId} for UserId {UserId}", addressId, userId);

            await _producer.PublishAsync("user.address.deleted", new
            {
                AddressId = ent.Id,
                UserId = ent.UserId,
                Timestamp = DateTime.UtcNow
            });

            return true;
        }
    }
}


