using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartInventory.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TestController : ControllerBase
{
    [HttpGet("secure")]
    public IActionResult Secure()
    {
        return Ok(new
        {
            message = "JWT Authentication is working!",
            user = User.Identity?.Name
        });
    }

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public IActionResult Admin()
    {
        return Ok(new
        {
            message = "Admin authorization is working!"
        });
    }
}