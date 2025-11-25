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

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var c = await _service.GetByIdAsync(id);
        if (c == null) return NotFound();
        return Ok(c);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        var cat = new Category { Name = dto.Name };
        var created = await _service.CreateAsync(cat);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        var cat = new Category { Id = id, Name = dto.Name };
        var updated = await _service.UpdateAsync(cat);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok) return NotFound();
        return NoContent();
    }
}
