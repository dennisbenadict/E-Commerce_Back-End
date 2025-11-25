using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Models;

namespace ProductService.Infrastructure.Repositories
{
	public class OrderRepository : IOrderRepository
	{
		private readonly ProductDbContext _db;

		public OrderRepository(ProductDbContext db)
		{
			_db = db;
		}

		public async Task<Order?> GetByIdAsync(int id)
		{
			return await _db.Orders
				.Include(o => o.Items)
				.ThenInclude(i => i.Product)
				.FirstOrDefaultAsync(o => o.Id == id);
		}

		public async Task<IEnumerable<Order>> GetUserOrdersAsync(int userId)
		{
			return await _db.Orders
				.Where(o => o.UserId == userId)
				.Include(o => o.Items)
				.ToListAsync();
		}

		public async Task AddOrderAsync(Order order)
		{
			await _db.Orders.AddAsync(order);
		}

		public async Task SaveChangesAsync()
		{
			await _db.SaveChangesAsync();
		}
	}
}
