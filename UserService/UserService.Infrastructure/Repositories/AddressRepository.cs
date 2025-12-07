using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Persistence;

namespace UserService.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly UserServiceDbContext _db;
    public AddressRepository(UserServiceDbContext db) => _db = db;

    public async Task<Address?> GetByIdAsync(int id)
        => await _db.Addresses.FindAsync(id);

    public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
        => await _db.Addresses.Where(a => a.UserId == userId).ToListAsync();

    public async Task AddAsync(Address address)
        => await _db.Addresses.AddAsync(address);

    public async Task UpdateAsync(Address address)
        => _db.Addresses.Update(address);

    public async Task DeleteAsync(Address address)
        => _db.Addresses.Remove(address);

    public async Task SaveChangesAsync()
        => await _db.SaveChangesAsync();
}
