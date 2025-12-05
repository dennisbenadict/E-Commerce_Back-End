using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface ICartRepository
{
    Task<Cart?> GetCartByUserIdAsync(int userId);
    Task CreateCartAsync(Cart cart);

    Task<CartItem?> GetCartItemAsync(int cartId, int productId);
    Task AddCartItemAsync(CartItem item);
    Task UpdateCartItemAsync(CartItem item);
    Task RemoveCartItemAsync(CartItem item);

    Task ClearCartAsync(int cartId);

    Task SaveChangesAsync();
}


