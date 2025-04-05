using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly VisualNetworkContext _context;
    public UserController(VisualNetworkContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("users")]
    public async Task<IActionResult> GetAllUsers()
    {
      var users = await _context.Users.ToListAsync();

      return StatusCode(StatusCodes.Status200OK, new { data = users });
    }

  }
}
