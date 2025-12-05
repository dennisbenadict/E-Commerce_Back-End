using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();

    Task AddOrderAsync(Order order);

    Task UpdateOrderAsync(Order order);

    Task SaveChangesAsync();
}

