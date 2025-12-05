using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.Services;
using System.Security.Claims;

namespace ProductService.Api.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;
    public OrderController(OrderService orderService) => _orderService = orderService;

    private int GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            throw new Exception("User ID missing in token");
        return int.Parse(userId);
    }

    // Create order from cart (authenticated)
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder()
    {
        int userId = GetUserId();
        var order = await _orderService.CreateOrderFromCartAsync(userId);
        return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
    }

    // Get my orders
    [HttpGet("my")]
    [Authorize]
    public async Task<IActionResult> GetMyOrders()
    {
        int userId = GetUserId();
        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    // Admin: get all orders
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    // Admin: get order by id
    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null) return NotFound();
        return Ok(order);
    }

    [HttpPut("{id:int}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(int id)
    {
        int userId = GetUserId();
        var result = await _orderService.CancelOrderAsync(id, userId, false);
        if (!result) return BadRequest("Order cannot be cancelled");
        return Ok(new { message = "Order cancelled" });
    }

    [HttpPut("{id:int}/admin-cancel")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdminCancel(int id)
    {
        var result = await _orderService.CancelOrderAsync(id, 0, true);
        if (!result) return BadRequest("Admin cannot cancel this order");
        return Ok(new { message = "Order cancelled by admin" });
    }
}
