//namespace ProductService.Application.DTOs
//{
//    public class CartDto
//    {
//        public int Id { get; set; }
//        public List<CartItemDto> Items { get; set; } = new();
//    }
//}
namespace ProductService.Application.DTOs
{
    public class CartDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
    }
}

