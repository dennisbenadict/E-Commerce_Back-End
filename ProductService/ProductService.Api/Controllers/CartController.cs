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
    public async Task<IActionResult> GetCart()
    {
        int userId = GetUserId();
        var cart = await _cartService.GetCartAsync(userId); // returns CartDto now

        if (cart == null)
            return Ok(new CartDto { Items = new List<CartItemDto>() });

        return Ok(cart);
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        int userId = GetUserId();

        var updatedCart = await _cartService.AddToCartAsync(userId, dto.ProductId, dto.Quantity);

        return Ok(new { message = "Item added to cart", cart = updatedCart });
    }

    [HttpDelete("remove")]
    public async Task<IActionResult> Remove([FromBody] RemoveItemDto dto)
    {
        int userId = GetUserId();

        await _cartService.RemoveItemAsync(userId, dto.ProductId);

        var cart = await _cartService.GetCartAsync(userId);

        return Ok(new { message = "Item removed", cart = cart });
    }
}

