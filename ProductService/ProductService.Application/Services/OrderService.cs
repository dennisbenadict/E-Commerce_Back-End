using ProductService.Application.Interfaces;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public class OrderService
{
    private readonly IOrderRepository _repo;
    private readonly ICartRepository _cartRepo;

    public OrderService(IOrderRepository repo, ICartRepository cartRepo)
    {
        _repo = repo;
        _cartRepo = cartRepo;
    }

    public async Task<OrderResponseDto?> GetByIdAsync(int id)
    {
        var order = await _repo.GetByIdAsync(id);
        if (order == null) return null;
        return MapToDto(order);
    }

    public async Task<IEnumerable<OrderResponseDto>> GetUserOrdersAsync(int userId)
    {
        var orders = await _repo.GetUserOrdersAsync(userId);
        return orders.Select(MapToDto).ToList();
    }

    public async Task<IEnumerable<OrderResponseDto>> GetAllOrdersAsync()
    {
        var orders = await _repo.GetAllOrdersAsync();
        return orders.Select(MapToDto).ToList();
    }

    public async Task<OrderResponseDto> CreateOrderFromCartAsync(int userId)
    {
        var cart = await _cartRepo.GetCartByUserIdAsync(userId);
        if (cart == null || !cart.Items.Any()) throw new Exception("Cart is empty");

        // ensure product prices are loaded
        foreach (var i in cart.Items)
        {
            if (i.Product == null)
            {
                // attempt to reload product
                // this usually shouldn't happen because repository includes product,
                // but we keep safe-guard
                throw new Exception("Product info missing for cart item");
            }
        }

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

        return MapToDto(order);
    }

    private OrderResponseDto MapToDto(Order o)
    {
        return new OrderResponseDto
        {
            Id = o.Id,
            UserId = o.UserId,
            CreatedAt = o.CreatedAt,
            TotalAmount = o.TotalAmount,
            Items = o.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.Product?.Name ?? string.Empty,
                Quantity = i.Quantity,
                Price = i.Price,
                ImageUrls = i.Product?.ImageUrls ?? new List<string>()
            }).ToList()
        };
    }

    public async Task<bool> CancelOrderAsync(int orderId, int userId, bool isAdmin)
    {
        var order = await _repo.GetByIdAsync(orderId);
        if (order == null)
            return false;

        if (!isAdmin)
        {
            if (order.UserId != userId)
                return false;

            var cancellableStatuses = new[] { "Pending", "Confirmed" };

            if (!cancellableStatuses.Contains(order.Status))
                return false;
        }
        else
        {
            if (order.Status == "Cancelled")
                return false;
        }

        order.Status = "Cancelled";

        await _repo.UpdateOrderAsync(order);
        await _repo.SaveChangesAsync();

        return true;
    }
}
