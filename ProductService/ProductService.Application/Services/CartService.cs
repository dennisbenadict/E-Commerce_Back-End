using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public class CartService
{
    private readonly ICartRepository _cartRepo;
    private readonly IProductRepository _productRepo;

    public CartService(ICartRepository cartRepo, IProductRepository productRepo)
    {
        _cartRepo = cartRepo;
        _productRepo = productRepo;
    }

    public async Task<Cart> AddToCartAsync(int userId, int productId, int qty)
    {
        if (qty <= 0) throw new ArgumentException("Quantity must be > 0");

        var product = await _productRepo.GetByIdAsync(productId);
        if (product == null || !product.IsActive)
            throw new Exception("Product not found or inactive");

        var cart = await _cartRepo.GetCartByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart { UserId = userId };
            await _cartRepo.CreateCartAsync(cart);
            await _cartRepo.SaveChangesAsync();
        }

        var existingItem = await _cartRepo.GetCartItemAsync(cart.Id, productId);

        if (existingItem != null)
        {
            existingItem.Quantity += qty;
            await _cartRepo.UpdateCartItemAsync(existingItem);
        }
        else
        {
            var newItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = productId,
                Quantity = qty
            };
            await _cartRepo.AddCartItemAsync(newItem);
        }

        await _cartRepo.SaveChangesAsync();

        // reload cart with product navigation
        return await _cartRepo.GetCartByUserIdAsync(userId) ?? cart;
    }

    public async Task<Cart?> GetCartAsync(int userId)
    {
        return await _cartRepo.GetCartByUserIdAsync(userId);
    }

    public async Task RemoveItemAsync(int userId, int productId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        if (cart == null) return;

        var item = await _cartRepo.GetCartItemAsync(cart.Id, productId);
        if (item == null) return;

        await _cartRepo.RemoveCartItemAsync(item);
        await _cartRepo.SaveChangesAsync();
    }
}
