namespace ProductService.Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<string> ImageUrl { get; set; } = new();
    public List<string> Size { get; set; } = new();
    public string Gender { get; set; } = null!; // Men, Women, Unisex

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

