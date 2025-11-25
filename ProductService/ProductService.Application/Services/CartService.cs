using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class CartService
    {
        private readonly ICartRepository _repo;
        private readonly IProductRepository _productRepo;

        public CartService(ICartRepository repo, IProductRepository productRepo)
        {
            _repo = repo;
            _productRepo = productRepo;
        }

        public async Task<Cart?> GetUserCartAsync(int userId)
        {
            return await _repo.GetUserCartAsync(userId);
        }

        public async Task AddToCartAsync(int userId, int productId, int qty)
        {
            var product = await _productRepo.GetByIdAsync(productId);
            if (product == null) throw new Exception("Product not found");

            var cart = await _repo.GetUserCartAsync(userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    Items = new List<CartItem>()
                };
            }

            cart.Items.Add(new CartItem
            {
                ProductId = productId,
                Quantity = qty,
                CartId = cart.Id
            });

            await _repo.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int itemId)
        {
            await _repo.RemoveItemAsync(itemId);
            await _repo.SaveChangesAsync();
        }

        public async Task ClearCartAsync(int cartId)
        {
            await _repo.ClearCartAsync(cartId);
            await _repo.SaveChangesAsync();
        }
    }
}
