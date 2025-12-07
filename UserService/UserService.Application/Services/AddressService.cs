using UserService.Application.DTOs;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;

namespace UserService.Application.Services;

public class AddressService : IAddressService
{
    private readonly IAddressRepository _repo;
    public AddressService(IAddressRepository repo) => _repo = repo;

    public async Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId)
    {
        var list = await _repo.GetByUserIdAsync(userId);
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
        if (ent == null || ent.UserId != userId) return null;

        ent.FullName = dto.FullName;
        ent.Street = dto.Street;
        ent.City = dto.City;
        ent.State = dto.State;
        ent.ZipCode = dto.ZipCode;
        ent.Country = dto.Country;
        ent.Phone = dto.Phone;

        await _repo.UpdateAsync(ent);
        await _repo.SaveChangesAsync();

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
        if (ent == null || ent.UserId != userId) return false;

        await _repo.DeleteAsync(ent);
        await _repo.SaveChangesAsync();
        return true;
    }
}
