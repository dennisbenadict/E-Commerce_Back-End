namespace ProductService.Application.DTOs;

public class CreateProductDto
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public List<string> Sizes { get; set; } = new();
    public List<string> ImageUrls { get; set; } = new();
    public string Gender { get; set; } = "Unisex";
    public bool IsActive { get; set; } = true;
}
