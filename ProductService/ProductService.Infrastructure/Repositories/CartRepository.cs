using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Models;

namespace ProductService.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ProductDbContext _db;
    public CartRepository(ProductDbContext db) => _db = db;

    public async Task<Cart?> GetCartByUserIdAsync(int userId)
    {
        return await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task CreateCartAsync(Cart cart) => await _db.Carts.AddAsync(cart);

    public async Task<CartItem?> GetCartItemAsync(int cartId, int productId)
    {
        return await _db.CartItems
            .Include(i => i.Product)
            .FirstOrDefaultAsync(i => i.CartId == cartId && i.ProductId == productId);
    }

    public async Task AddCartItemAsync(CartItem item) => await _db.CartItems.AddAsync(item);

    public async Task UpdateCartItemAsync(CartItem item)
    {
        _db.CartItems.Update(item);
        await Task.CompletedTask;
    }

    public async Task RemoveCartItemAsync(CartItem item)
    {
        _db.CartItems.Remove(item);
        await Task.CompletedTask;
    }

    public async Task ClearCartAsync(int cartId)
    {
        var items = _db.CartItems.Where(i => i.CartId == cartId);
        _db.CartItems.RemoveRange(items);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
