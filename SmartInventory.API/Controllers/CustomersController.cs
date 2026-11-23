
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartInventory.Application.DTOs.Customers;
using SmartInventory.Application.Interfaces;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _service;

    public CustomersController(ICustomerService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _service.GetByIdAsync(id);

        if (customer == null)
            return NotFound(new { message = "Customer not found." });

        return Ok(customer);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Create(CreateCustomerDto dto)
    {
        try
        {
            var customer = await _service.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.CustomerId },
                customer);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Update(
        int id, UpdateCustomerDto dto)
    {
        try
        {
            return Ok(await _service.UpdateAsync(id, dto));
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message == "Customer not found."
                ? NotFound(new { message = ex.Message })
                : Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);

            return Ok(new
            {
                message = "Customer deleted successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return ex.Message == "Customer not found."
                ? NotFound(new { message = ex.Message })
                : Conflict(new { message = ex.Message });
        }
    }
}
