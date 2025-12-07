using UserService.Application.DTOs;

namespace UserService.Application.Interfaces;

public interface IAddressService
{
    Task<IEnumerable<AddressDto>> GetAddressesAsync(int userId);
    Task<AddressDto> CreateAddressAsync(int userId, CreateAddressDto dto);
    Task<AddressDto?> UpdateAddressAsync(int userId, int addressId, CreateAddressDto dto);
    Task<bool> DeleteAddressAsync(int userId, int addressId);
}
