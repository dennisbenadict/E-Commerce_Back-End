using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Models;

namespace ProductService.Infrastructure.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ProductDbContext _db;

        public CartRepository(ProductDbContext db)
        {
            _db = db;
        }

        public async Task<Cart?> GetUserCartAsync(int userId)
        {
            return await _db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        public async Task AddItemAsync(CartItem item)
        {
            await _db.CartItems.AddAsync(item);
        }

        public async Task RemoveItemAsync(int itemId)
        {
            var item = await _db.CartItems.FindAsync(itemId);
            if (item != null)
                _db.CartItems.Remove(item);
        }

        public async Task ClearCartAsync(int cartId)
        {
            var items = await _db.CartItems
                .Where(i => i.CartId == cartId)
                .ToListAsync();

            _db.CartItems.RemoveRange(items);
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }
    }
}
