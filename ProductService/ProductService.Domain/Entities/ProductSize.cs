namespace ProductService.Domain.Entities;

public class ProductSize
{
    public int Id { get; set; }
    public string Value { get; set; } = null!; // e.g. "M", "9", "42"

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}
