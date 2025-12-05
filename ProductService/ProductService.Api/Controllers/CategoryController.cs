using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;
using ProductService.Domain.Entities;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _service;
    public CategoryController(CategoryService service) => _service = service;

    // ⭐ Public — Everyone can view categories
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    // ⭐ Public — Everyone can get a specific category
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _service.GetByIdAsync(id);
        if (category == null) return NotFound();
        return Ok(category);
    }

    // 🔒 Admin Only — Create category
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var category = new Category { Name = dto.Name };
        var created = await _service.CreateAsync(category);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    // 🔒 Admin Only — Update category
    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        var category = new Category { Id = id, Name = dto.Name };
        var updated = await _service.UpdateAsync(category);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // 🔒 Admin Only — Delete category
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
