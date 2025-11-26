namespace ProductService.Domain.Entities;
using System.Collections.Generic;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public decimal Price { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public List<string> ImageUrls { get; set; } = new();
    public List<string> Sizes { get; set; } = new();

    public string Gender { get; set; } = "Unisex";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}



