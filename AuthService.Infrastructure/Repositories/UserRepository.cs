using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task BlockAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.IsBlocked = true;
            await _context.SaveChangesAsync();
        }

        public async Task UnblockAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.IsBlocked = false;
            await _context.SaveChangesAsync();
        }

        public async Task MakeAdminAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.IsAdmin = true;
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAdminAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return;

            user.IsAdmin = false;
            await _context.SaveChangesAsync();
        }
    }
}

