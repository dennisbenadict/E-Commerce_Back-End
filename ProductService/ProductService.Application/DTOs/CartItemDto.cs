namespace ProductService.Application.DTOs
{
	public class CartItemDto
	{
		public int ProductId { get; set; }
		public int Quantity { get; set; }

		public ProductDto Product { get; set; } = null!;
	}
}
