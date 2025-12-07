using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using UserService.Application.DTOs;
using UserService.Application.Interfaces;

namespace UserService.Api.Controllers;

[ApiController]
[Route("api/users/addresses")]
[Authorize]
public class AddressController : ControllerBase
{
    private readonly IAddressService _service;
    public AddressController(IAddressService service) => _service = service;

    private int GetUserId()
    {
        var v = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(v)) throw new Exception("User id missing in token");
        return int.Parse(v);
    }


    [HttpGet]
    [ProducesResponseType(typeof(List<AddressDto>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var id = GetUserId();
        var list = await _service.GetAddressesAsync(id);

        return Ok(new
        {
            message = list.Any() ? "Addresses fetched successfully" : "No addresses found",
            count = list.Count(),
            addresses = list
        });
    }


    [HttpPost]
    [ProducesResponseType(typeof(AddressDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Create([FromBody] CreateAddressDto dto)
    {
        var id = GetUserId();
        var added = await _service.CreateAddressAsync(id, dto);

        return CreatedAtAction(nameof(GetAll), new { id = added.Id }, new
        {
            message = "Address created successfully",
            address = added
        });
    }


    [HttpPut("{addressId:int}")]
    [ProducesResponseType(typeof(AddressDto), 200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Update(int addressId, [FromBody] CreateAddressDto dto)
    {
        var id = GetUserId();
        var updated = await _service.UpdateAddressAsync(id, addressId, dto);

        if (updated == null)
            return NotFound(new { message = "Address not found" });

        return Ok(new
        {
            message = "Address updated successfully",
            address = updated
        });
    }


    [HttpDelete("{addressId:int}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Delete(int addressId)
    {
        var id = GetUserId();
        var ok = await _service.DeleteAddressAsync(id, addressId);

        if (!ok)
            return NotFound(new { message = "Address not found or already deleted" });

        return NoContent();
    }
}

