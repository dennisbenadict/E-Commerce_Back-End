using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(int id);
    Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
    Task AddAsync(Address address);
    Task UpdateAsync(Address address);
    Task DeleteAsync(Address address);
    Task SaveChangesAsync();
}
