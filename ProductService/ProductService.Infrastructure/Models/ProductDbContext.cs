using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.Text.Json;

namespace ProductService.Infrastructure.Models;

public class ProductDbContext : DbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options) { }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(200);
        });

        // Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                  .IsRequired()
                  .HasMaxLength(200);

            entity.Property(x => x.Description)
                  .HasMaxLength(2000);

            entity.Property(x => x.Gender)
                  .HasMaxLength(20);

            entity.Property(x => x.Price)
                  .HasColumnType("decimal(18,2)");

            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);

            // JSON serialization for lists (ImageUrls and Sizes)
            entity.Property(x => x.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v)!);

            entity.Property(x => x.Sizes)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : JsonSerializer.Deserialize<List<string>>(v)!);

            entity.HasOne(x => x.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(x => x.CategoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Cart
        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UserId).IsRequired();
        });

        // CartItem
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.HasOne(x => x.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(x => x.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                  .WithMany()
                  .HasForeignKey(x => x.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Order
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.UserId).IsRequired();
            entity.Property(x => x.TotalAmount)
                  .HasColumnType("decimal(18,2)");
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Price)
                  .HasColumnType("decimal(18,2)");

            entity.HasOne(x => x.Order)
                  .WithMany(o => o.Items)
                  .HasForeignKey(x => x.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Product)
                  .WithMany()
                  .HasForeignKey(x => x.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

