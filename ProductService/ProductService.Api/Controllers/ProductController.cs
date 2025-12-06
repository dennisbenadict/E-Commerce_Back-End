//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using ProductService.Application.DTOs;
//using ProductService.Application.Services;

//namespace ProductService.Api.Controllers;

//[ApiController]
//[Route("api/products")]
//public class ProductController : ControllerBase
//{
//    private readonly ProductService.Application.Services.ProductService _service;
//    public ProductController(ProductService.Application.Services.ProductService service) => _service = service;

//    [HttpGet]
//    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

//    [HttpGet("{id:int}")]
//    public async Task<IActionResult> Get(int id)
//    {
//        var product = await _service.GetByIdAsync(id);
//        if (product == null) return NotFound();
//        return Ok(product);
//    }

//    // Admin only
//    [HttpPost]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
//    {
//        var created = await _service.CreateAsync(dto);
//        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
//    }

//    [HttpPut("{id:int}")]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
//    {
//        var updated = await _service.UpdateAsync(id, dto);
//        if (updated == null) return NotFound();
//        return Ok(updated);
//    }

//    [HttpDelete("{id:int}")]
//    [Authorize(Roles = "Admin")]
//    public async Task<IActionResult> Delete(int id, [FromQuery] bool hard = true)
//    {
//        var ok = await _service.DeleteAsync(id, hard);
//        if (!ok) return NotFound();
//        return NoContent();
//    }

//    [HttpGet("category/{catId:int}")]
//    public async Task<IActionResult> GetByCategory(int catId)
//        => Ok(await _service.GetByCategoryAsync(catId));
//}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductService.Application.Services.ProductService _service;
    public ProductController(ProductService.Application.Services.ProductService service)
        => _service = service;


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(int id)
    {
        var product = await _service.GetByIdAsync(id);

        if (product == null)
            return NotFound(new { message = "Product not found" });

        return Ok(product);
    }


    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            var created = await _service.CreateAsync(dto);

            return CreatedAtAction(nameof(Get), new { id = created.Id }, new
            {
                message = "Product created successfully",
                product = created
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);

        if (updated == null)
            return NotFound(new { message = "Product not found" });

        return Ok(new
        {
            message = "Product updated successfully",
            product = updated
        });
    }


    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(int id, [FromQuery] bool hard = true)
    {
        var ok = await _service.DeleteAsync(id, hard);

        if (!ok)
            return NotFound(new { message = "Product not found" });

        return NoContent();
    }


    [HttpGet("category/{catId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCategory(int catId)
        => Ok(new
        {
            message = "Products fetched by category",
            products = await _service.GetByCategoryAsync(catId)
        });
}
