using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  [Authorize]
  public class BaseUserController : ControllerBase
  {

    protected int? GetLoggedInUserId()
    {
      var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
      if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
      {
        return userId;
      }
      return null;
    }

    protected string? GetLoggedInUsername()
    {
      return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
    }

  }
}
