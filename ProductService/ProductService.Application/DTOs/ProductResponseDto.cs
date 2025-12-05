namespace ProductService.Application.DTOs;

public class ProductResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public List<string> Sizes { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public string Gender { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
