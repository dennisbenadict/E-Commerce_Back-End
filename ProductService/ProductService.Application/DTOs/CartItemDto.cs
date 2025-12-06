//namespace ProductService.Application.DTOs
//{
//    public class CartItemDto
//    {
//        public int ProductId { get; set; }
//        public int Quantity { get; set; }

//        public ProductDto Product { get; set; } = null!;
//    }
//}

namespace ProductService.Application.DTOs
{
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public List<string> ImageUrls { get; set; } = new();
    }
}
