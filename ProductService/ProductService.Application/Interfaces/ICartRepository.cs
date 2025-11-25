using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces
{
	public interface ICartRepository
	{
		Task<Cart?> GetUserCartAsync(int userId);
		Task AddItemAsync(CartItem item);
		Task RemoveItemAsync(int itemId);
		Task ClearCartAsync(int cartId);

		Task SaveChangesAsync();
	}
}
