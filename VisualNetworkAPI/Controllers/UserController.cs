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
      var publicUsers = users.Select(u => new UserPublicDto
      {
        Id = u.Id,
        User = u.User1,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        DateBirth = u.DateBirth,
        Active = u.Active,
        Phone = u.Phone,
        Address = u.Address,
        Genre = u.Genre,
        CreatedBy = u.CreatedBy,
        CreatedDate = u.CreatedDate,
        LastUpdate = u.LastUpdate

      }).ToList();

      return StatusCode(StatusCodes.Status200OK, new { data = publicUsers });
    }

    [HttpGet]
    [Route("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _context.Users.FindAsync(id);

      if (user == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var publicUser = new UserPublicDto
      {
        Id = user.Id,
        User = user.User1,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        DateBirth = user.DateBirth,
        Active = user.Active,
        Phone = user.Phone,
        Address = user.Address,
        Genre = user.Genre,
        CreatedBy = user.CreatedBy,
        CreatedDate = user.CreatedDate,
        LastUpdate = user.LastUpdate
      };

      return Ok(new { data = publicUser });
    }

    [HttpPut]
    [Route("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userToUpdate = await _context.Users.FindAsync(id);

      if (userToUpdate == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      userToUpdate.FirstName = user.FirstName ?? userToUpdate.FirstName;
      userToUpdate.LastName = user.LastName ?? userToUpdate.LastName;
      userToUpdate.Email = user.Email ?? userToUpdate.Email;
      userToUpdate.Phone = user.Phone ?? userToUpdate.Phone;
      userToUpdate.Address = user.Address ?? userToUpdate.Address;
      userToUpdate.Genre = user.Genre ?? userToUpdate.Genre;
      userToUpdate.LastUpdate = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      var publicUser = new UserPublicDto
      {
        Id = userToUpdate.Id,
        User = userToUpdate.User1,
        Email = userToUpdate.Email,
        FirstName = userToUpdate.FirstName,
        LastName = userToUpdate.LastName,
        DateBirth = userToUpdate.DateBirth,
        Active = userToUpdate.Active,
        Phone = userToUpdate.Phone,
        Address = userToUpdate.Address,
        Genre = userToUpdate.Genre,
        CreatedBy = userToUpdate.CreatedBy,
        CreatedDate = userToUpdate.CreatedDate,
        LastUpdate = userToUpdate.LastUpdate
      };

      return Ok(new { data = publicUser });
    }

    [HttpDelete]
    [Route("users/{id}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
      var userToDeactivate = await _context.Users.FindAsync(id);

      if (userToDeactivate == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      if (!userToDeactivate.Active)
      {
        return BadRequest(new { message = "El usuario ya está inactivo" });
      }

      userToDeactivate.Active = false;
      userToDeactivate.LastUpdate = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Ok(new { message = $"Usuario con ID {id} inactivado exitosamente" });
    }


  }
}
