using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Services;
using System.Security.Claims;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly CartService _cartService;
    public CartController(CartService cartService) => _cartService = cartService;

    private int GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new Exception("User ID missing in token");
        return int.Parse(userId);
    }


    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCart()
    {
        int userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId);

        if (cart == null)
            return Ok(new { message = "Cart is empty", items = new List<CartItemDto>() });

        return Ok(cart);
    }


    [HttpPost("add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        int userId = GetUserId();

        if (dto.Quantity <= 0)
            return BadRequest(new { message = "Quantity must be greater than zero" });

        try
        {
            var updatedCart = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);

            return Ok(new
            {
                message = "Item added to cart successfully",
                cart = updatedCart
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    [HttpDelete("remove")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Remove([FromBody] RemoveItemDto dto)
    {
        int userId = GetUserId();

        await _cartService.RemoveItemAsync(userId, dto.ProductId);
        var cart = await _cartService.GetCartAsync(userId);

        return Ok(new
        {
            message = "Item removed from cart successfully",
            cart = cart
        });
    }
}
