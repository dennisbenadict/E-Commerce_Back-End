using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services
{
    public class OrderService
    {
        private readonly IOrderRepository _repo;
        private readonly ICartRepository _cartRepo;

        public OrderService(IOrderRepository repo, ICartRepository cartRepo)
        {
            _repo = repo;
            _cartRepo = cartRepo;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
        {
            return await _repo.GetUserOrdersAsync(userId);
        }

        public async Task<Order> CreateOrderFromCartAsync(int userId)
        {
            var cart = await _cartRepo.GetUserCartAsync(userId);
            if (cart == null || !cart.Items.Any())
                throw new Exception("Cart is empty");

            var order = new Order
            {
                UserId = userId,
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price),
                Items = cart.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Quantity = i.Quantity,
                    Price = i.Product.Price
                }).ToList()
            };

            await _repo.AddOrderAsync(order);
            await _repo.SaveChangesAsync();

            await _cartRepo.ClearCartAsync(cart.Id);
            await _cartRepo.SaveChangesAsync();

            return order;
        }
    }
}
