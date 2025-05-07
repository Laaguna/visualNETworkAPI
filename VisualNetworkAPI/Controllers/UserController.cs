using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Board;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class UserController : BaseUserController
  {
    private readonly VisualNetworkContext _context;
    public UserController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpGet]
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

    [HttpGet("{id}")]
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

    [HttpPut("{id}")]
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
      userToUpdate.Avatar = user.Avatar ?? userToUpdate.Avatar;

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
        LastUpdate = userToUpdate.LastUpdate,
        Avatar = userToUpdate.Avatar,
      };

      return Ok(new { data = publicUser });
    }

    [HttpPut("{id}/profile-image")]
    public async Task<IActionResult> UpdateProfileImage(int id, IFormFile image)
    {
      if (image == null || image.Length == 0)
      {
        return BadRequest("Debe subir una imagen.");
      }

      var userToUpdate = await _context.Users.FindAsync(id);
      if (userToUpdate == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      // Verificar que el usuario actual sea el dueño del perfil
      var currentUserId = GetLoggedInUserId();
      if (userToUpdate.Id != currentUserId)
      {
        return Forbid(new { message = "No tienes permiso para actualizar este perfil" }.ToString());
      }

      // Eliminar imagen anterior si no es la imagen por defecto
      if (!string.IsNullOrEmpty(userToUpdate.Avatar) &&
          !userToUpdate.Avatar.EndsWith("/blank.svg") &&
          !userToUpdate.Avatar.EndsWith("/default.png"))
      {
        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot",
            userToUpdate.Avatar.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (System.IO.File.Exists(oldImagePath))
        {
          try
          {
            System.IO.File.Delete(oldImagePath);
          }
          catch (Exception)
          {
            // Loguear error pero continuar
          }
        }
      }

      // Guardar nueva imagen
      var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
      var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars", fileName);

      using (var stream = new FileStream(imagePath, FileMode.Create))
      {
        await image.CopyToAsync(stream);
      }

      // Actualizar referencia en el usuario
      userToUpdate.Avatar = $"/avatars/{fileName}";
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
        LastUpdate = userToUpdate.LastUpdate,
        Avatar = userToUpdate.Avatar  
      };

      return Ok(new { data = publicUser });
    }

    [HttpDelete("{id}")]
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

    [HttpGet("{userId}/posts")] 
    public async Task<IActionResult> GetUserPosts(int userId)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var userPosts = await _context.Posts
          .Where(p => p.CreatedBy == userId)
          .Select(p => new Post 
          {
            Id = p.Id,
            Title = p.Title,
            Description = p.Description,
            ImageUrls = p.ImageUrls,
            JsonPersonalizacion = p.JsonPersonalizacion,
            CreatedDate = p.CreatedDate,
          })
          .ToListAsync();

      return Ok(new { data = userPosts });
    }

    [HttpGet("{userId}/boards")]
    public async Task<IActionResult> GetUserBoards(int userId)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var userBoards = await _context.Boards
        .Where(p => p.CreatedBy == userId.ToString())
        .Select(p => new PublicBoardDTO
        {
          Id = p.Id,
          Description = p.Description,
          Decoration = p.Decoration,
          CreatedDate = p.CreatedDate,
          LastUpdate = p.LastUpdate,
        }).ToListAsync();

      return Ok(new { data = userBoards });
    }

    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetUserFollowers(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var followerIds = await _context.Followers
          .Where(f => f.FollowedId == id)
          .Select(f => f.FollowerId)
          .ToListAsync();

      var followersData = new List<object>();
      foreach (var followerId in followerIds)
      {
        var follower = await _context.Users.FindAsync(followerId);
        if (follower != null)
        {
          followersData.Add(new
          {
            Follower = new UserPublicDto
            {
              Id = follower.Id,
              User = follower.User1,
              Email = follower.Email,
              FirstName = follower.FirstName,
              LastName = follower.LastName,
              DateBirth = follower.DateBirth,
              Active = follower.Active,
              Phone = follower.Phone,
              Address = follower.Address,
              Genre = follower.Genre,
              CreatedBy = follower.CreatedBy,
              CreatedDate = follower.CreatedDate,
              LastUpdate = follower.LastUpdate
            },
            FollowDate = await _context.Followers
                  .Where(f => f.FollowerId == followerId && f.FollowedId == id)
                  .Select(f => f.FollowDate)
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = followersData });
    }

    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetUserFollowing(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var followingIds = await _context.Followers
          .Where(f => f.FollowerId == id)
          .Select(f => f.FollowedId)
          .ToListAsync();

      var followingData = new List<object>();
      foreach (var followingId in followingIds)
      {
        var followedUser = await _context.Users.FindAsync(followingId);
        if (followedUser != null)
        {
          followingData.Add(new
          {
            Following = new UserPublicDto
            {
              Id = followedUser.Id,
              User = followedUser.User1,
              Email = followedUser.Email,
              FirstName = followedUser.FirstName,
              LastName = followedUser.LastName,
              DateBirth = followedUser.DateBirth,
              Active = followedUser.Active,
              Phone = followedUser.Phone,
              Address = followedUser.Address,
              Genre = followedUser.Genre,
              CreatedBy = followedUser.CreatedBy,
              CreatedDate = followedUser.CreatedDate,
              LastUpdate = followedUser.LastUpdate
            },
            FollowDate = await _context.Followers
                  .Where(f => f.FollowerId == id && f.FollowedId == followingId)
                  .Select(f => f.FollowDate)
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = followingData });
    }

    [HttpPost("{id}/followers")]
    public async Task<IActionResult> FollowUser(int id)
    {
      var userToFollow = await _context.Users.FindAsync(id);
      if (userToFollow == null)
      {
        return NotFound(new { message = "Usuario a seguir no encontrado" });
      }

      var currentUserId = GetLoggedInUserId(); // Obtiene el ID del usuario que está haciendo la solicitud
      if (!currentUserId.HasValue)
      {
        return Unauthorized(new { message = "Usuario no autenticado" });
      }

      if (currentUserId == id)
      {
        return BadRequest(new { message = "No puedes seguirte a ti mismo" });
      }

      // Verifica si ya existe la relación de seguimiento
      var existingFollow = await _context.Followers.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowedId == id);
      if (existingFollow != null)
      {
        return Conflict(new { message = "Ya estás siguiendo a este usuario" });
      }

      var newFollow = new Follower
      {
        FollowerId = currentUserId.Value,
        FollowedId = id,
        FollowDate = DateTime.UtcNow
      };

      _context.Followers.Add(newFollow);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetUserFollowers), new { id = id }, new { message = "Usuario seguido exitosamente" }); // Puedes devolver información adicional si lo deseas
    }

    [HttpDelete("{id}/followers")]
    public async Task<IActionResult> UnfollowUser(int id)
    {
      var userToUnfollow = await _context.Users.FindAsync(id);
      if (userToUnfollow == null)
      {
        return NotFound(new { message = "Usuario a dejar de seguir no encontrado" });
      }

      var currentUserId = GetLoggedInUserId(); // Obtiene el ID del usuario que está haciendo la solicitud
      if (!currentUserId.HasValue)
      {
        return Unauthorized(new { message = "Usuario no autenticado" });
      }


      // Verifica si existe la relación de seguimiento
      var followToDelete = await _context.Followers.FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowedId == id);
      if (followToDelete == null)
      {
        return NotFound(new { message = "No estás siguiendo a este usuario" });
      }

      _context.Followers.Remove(followToDelete);
      await _context.SaveChangesAsync();

      return NoContent(); // 204 No Content para indicar éxito sin cuerpo de respuesta
    }

    [HttpGet("{id}/liked-posts")]
    public async Task<IActionResult> GetUserLikedPosts(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var likedPostIds = await _context.LikePosts
          .Where(lp => lp.UserId == id)
          .Select(lp => lp.PostId)
          .ToListAsync();

      var likedPostsData = new List<object>();
      foreach (var postId in likedPostIds)
      {
        var post = await _context.Posts.FindAsync(postId);
        if (post != null)
        {
          likedPostsData.Add(new
          {
            Post = new Post
            {
              Id = post.Id,
              Title = post.Title,
              Description = post.Description,
              PostTags = post.PostTags,
              ImageUrls = post.ImageUrls,
              JsonPersonalizacion = post.JsonPersonalizacion,
              CreatedBy = post.CreatedBy,
              CreatedDate = post.CreatedDate,
              LastUpdate = post.LastUpdate
            },
            LikedDate = await _context.LikePosts
                  .Where(lp => lp.UserId == id && lp.PostId == postId)
                  .Select(lp => lp.CreatedDate)
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = likedPostsData });
    }
  }
}
