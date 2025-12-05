using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Models;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ProductDbContext _db;
    public ProductRepository(ProductDbContext db) => _db = db;

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _db.Products
            .Where(p => !p.IsDeleted && p.IsActive)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);
    }

    public async Task<IEnumerable<Product>> GetByCategoryAsync(int categoryId)
    {
        return await _db.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.IsActive)
            .ToListAsync();
    }

    public async Task AddAsync(Product product) => await _db.Products.AddAsync(product);

    public Task UpdateAsync(Product product)
    {
        _db.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var prod = await _db.Products.FindAsync(id);
        if (prod != null)
            _db.Products.Remove(prod); // per user's choice hard delete allowed
    }

    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
