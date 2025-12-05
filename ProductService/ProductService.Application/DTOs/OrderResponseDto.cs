namespace ProductService.Application.DTOs;

public class OrderResponseDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public string Status { get; set; } = "Pending";

}
