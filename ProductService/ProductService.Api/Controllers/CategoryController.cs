//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ProductService.Application.DTOs;
//using ProductService.Application.Services;
//using ProductService.Domain.Entities;

//namespace ProductService.Api.Controllers;

//[ApiController]
//[Route("api/categories")]
//public class CategoryController : ControllerBase
//{
//    private readonly CategoryService _service;
//    public CategoryController(CategoryService service) => _service = service;

//    // ⭐ Public — Everyone can view categories
//    [HttpGet]
//    public async Task<IActionResult> GetAll() =>
//        Ok(await _service.GetAllAsync());

//    // ⭐ Public — Everyone can get a specific category
//    [HttpGet("{id:int}")]
//    public async Task<IActionResult> Get(int id)
//    {
//        var category = await _service.GetByIdAsync(id);
//        if (category == null) return NotFound();
//        return Ok(category);
//    }

//    // 🔒 Admin Only — Create category
//    [HttpPost]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
//    {
//        var category = new Category { Name = dto.Name };
//        var created = await _service.CreateAsync(category);
//        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
//    }

//    // 🔒 Admin Only — Update category
//    [HttpPut("{id:int}")]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
//    {
//        var category = new Category { Id = id, Name = dto.Name };
//        var updated = await _service.UpdateAsync(category);
//        if (updated == null) return NotFound();
//        return Ok(updated);
//    }

//    // 🔒 Admin Only — Delete category
//    [HttpDelete("{id:int}")]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Delete(int id)
//    {
//        var ok = await _service.DeleteAsync(id);
//        if (!ok) return NotFound();
//        return NoContent();
//    }
//}

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


    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Category>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());


    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var category = await _service.GetByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Category not found" });

        return Ok(category);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Category name cannot be empty" });

        var category = new Category { Name = dto.Name };
        var created = await _service.CreateAsync(category);

        return CreatedAtAction(nameof(Get), new { id = created.Id }, new
        {
            message = "Category created successfully",
            category = created
        });
    }


    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Category), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest(new { message = "Category name cannot be empty" });

        var category = new Category { Id = id, Name = dto.Name };
        var updated = await _service.UpdateAsync(category);

        if (updated == null)
            return NotFound(new { message = "Category not found" });

        return Ok(new
        {
            message = "Category updated successfully",
            category = updated
        });
    }


    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _service.DeleteAsync(id);
        if (!ok)
            return NotFound(new { message = "Category not found" });

        return NoContent();
    }
}
