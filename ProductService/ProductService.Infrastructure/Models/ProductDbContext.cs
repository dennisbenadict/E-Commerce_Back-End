using Microsoft.EntityFrameworkCore;
using ProductService.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace ProductService.Infrastructure.Models
{
    public class ProductDbContext : DbContext
    {
        public ProductDbContext(DbContextOptions<ProductDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name)
                      .IsRequired()
                      .HasMaxLength(200);
            });


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

                entity.Property(x => x.Size)
                      .HasMaxLength(20);

                entity.Property(x => x.ImageUrl)
                      .HasMaxLength(500);

                entity.Property(x => x.Price)
                      .HasColumnType("decimal(18,2)");


                entity.HasOne(x => x.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(x => x.CategoryId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).IsRequired();
            });


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
                      .OnDelete(DeleteBehavior.Restrict); // Product should not auto delete cart
            });


            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(x => x.Id);
                entity.Property(x => x.UserId).IsRequired();
                entity.Property(x => x.TotalAmount).HasColumnType("decimal(18,2)");
            });


            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");

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
}

