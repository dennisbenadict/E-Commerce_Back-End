using ProductService.Application.Interfaces;
using ProductService.Application.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Services;

public class ProductService
{
    private readonly IProductRepository _repo;
    public ProductService(IProductRepository repo) => _repo = repo;

    public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
    {
        var list = await _repo.GetAllAsync();
        return list.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponseDto?> GetByIdAsync(int id)
    {
        var p = await _repo.GetByIdAsync(id);
        return p == null ? null : MapToResponse(p);
    }

    public async Task<IEnumerable<ProductResponseDto>> GetByCategoryAsync(int categoryId)
    {
        var list = await _repo.GetByCategoryAsync(categoryId);
        return list.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
    {
        ValidateCreate(dto);
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            CategoryId = dto.CategoryId,
            Sizes = dto.Sizes ?? new(),
            ImageUrls = dto.ImageUrls ?? new(),
            Gender = dto.Gender,
            IsActive = dto.IsActive
        };

        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();

        return MapToResponse(product);
    }

    public async Task<ProductResponseDto?> UpdateAsync(int id, UpdateProductDto dto)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return null;

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.Price = dto.Price;
        existing.CategoryId = dto.CategoryId;
        existing.Sizes = dto.Sizes ?? new();
        existing.ImageUrls = dto.ImageUrls ?? new();
        existing.Gender = dto.Gender;
        existing.IsActive = dto.IsActive;

        await _repo.UpdateAsync(existing);
        await _repo.SaveChangesAsync();

        return MapToResponse(existing);
    }

    public async Task<bool> DeleteAsync(int id, bool hardDelete = true)
    {
        var existing = await _repo.GetByIdAsync(id);
        if (existing == null) return false;

        if (hardDelete)
        {
            await _repo.DeleteAsync(id);
        }
        else
        {
            existing.IsDeleted = true;
            await _repo.UpdateAsync(existing);
        }

        await _repo.SaveChangesAsync();
        return true;
    }

    private void ValidateCreate(CreateProductDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Product name is required");
        if (dto.Price < 0) throw new ArgumentException("Price cannot be negative");
    }

    private ProductResponseDto MapToResponse(Product p)
    {
        return new ProductResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Price = p.Price,
            CategoryId = p.CategoryId,
            Sizes = p.Sizes ?? new(),
            ImageUrls = p.ImageUrls ?? new(),
            Gender = p.Gender,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt
        };
    }
}
