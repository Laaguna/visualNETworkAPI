using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Board;
using VisualNetworkAPI.Paginated;

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
public async Task<IActionResult> GetUserBoards(int userId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
{
    // Verificar si el usuario existe
    var userExists = await _context.Users
        .AsNoTracking()
        .AnyAsync(u => u.Id == userId);
        
    if (!userExists)
    {
        return NotFound(new { message = "Usuario no encontrado" });
    }
    
    // Aplicar filtro y paginación
    var paginatedUserBoards = await _context.Boards
        .AsNoTracking()
        .Where(p => p.CreatedBy == userId.ToString())
        .OrderByDescending(p => p.CreatedDate) // Ordenamiento consistente
        .Select(p => new PublicBoardDTO
        {
            Id = p.Id,
            Description = p.Description,
            Decoration = p.Decoration,
            CreatedDate = p.CreatedDate,
            LastUpdate = p.LastUpdate,
        })
        .ToPaginatedListAsync(pageIndex, pageSize);
    
    // Retornar datos paginados con metadatos de paginación
    return Ok(new 
    { 
        data = paginatedUserBoards.Items,
        pageIndex = paginatedUserBoards.PageIndex,
        pageSize = paginatedUserBoards.PageSize,
        totalCount = paginatedUserBoards.TotalCount,
        totalPages = paginatedUserBoards.TotalPages,
        hasPreviousPage = paginatedUserBoards.HasPreviousPage,
        hasNextPage = paginatedUserBoards.HasNextPage
    });
}

    [HttpGet("{id}/followers")]
    public async Task<IActionResult> GetUserFollowers(int id, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        // Verificar si el usuario existe
        var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == id);
        if (!userExists)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        
        // Consulta optimizada para paginación
        var followersQuery = _context.Followers
            .AsNoTracking()
            .Where(f => f.FollowedId == id)
            .OrderByDescending(f => f.FollowDate)
            .Select(f => new { 
                FollowerId = f.FollowerId, 
                FollowDate = f.FollowDate 
            });
        
        // Aplicar paginación
        var paginatedFollowers = await followersQuery.ToPaginatedListAsync(pageIndex, pageSize);
        
        if (!paginatedFollowers.Items.Any())
        {
            return Ok(new 
            { 
                data = new List<object>(),
                pageIndex = paginatedFollowers.PageIndex,
                pageSize = paginatedFollowers.PageSize,
                totalCount = paginatedFollowers.TotalCount,
                totalPages = paginatedFollowers.TotalPages,
                hasPreviousPage = paginatedFollowers.HasPreviousPage,
                hasNextPage = paginatedFollowers.HasNextPage
            });
        }
        
        // Extraer IDs de los seguidores para obtener sus datos en una sola consulta
        var followerIds = paginatedFollowers.Items.Select(f => f.FollowerId).ToList();
        
        // Cargar todos los usuarios en una sola consulta
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => followerIds.Contains(u.Id))
            .Select(u => new PublicUserRelationDTO
            {
                Id = u.Id,
                Avatar = u.Avatar,
                FirstName = u.FirstName,
                LastName = u.LastName,
                User1 = u.User1
            })
            .ToDictionaryAsync(u => u.Id);
        
        // Combinar datos de seguidores con sus fechas de seguimiento
        var followersData = paginatedFollowers.Items
            .Where(f => users.ContainsKey(f.FollowerId))
            .Select(f => new
            {
                Follower = users[f.FollowerId],
                FollowDate = f.FollowDate
            })
            .ToList();
        
        return Ok(new 
        { 
            data = followersData,
            pageIndex = paginatedFollowers.PageIndex,
            pageSize = paginatedFollowers.PageSize,
            totalCount = paginatedFollowers.TotalCount,
            totalPages = paginatedFollowers.TotalPages,
            hasPreviousPage = paginatedFollowers.HasPreviousPage,
            hasNextPage = paginatedFollowers.HasNextPage
        });
    }

    [HttpGet("{id}/following")]
    public async Task<IActionResult> GetUserFollowing(int id, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        // Verificar si el usuario existe
        var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == id);
        if (!userExists)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }
        
        // Consulta optimizada para paginación
        var followingQuery = _context.Followers
            .AsNoTracking()
            .Where(f => f.FollowerId == id)
            .OrderByDescending(f => f.FollowDate)
            .Select(f => new { 
                FollowedId = f.FollowedId, 
                FollowDate = f.FollowDate 
            });
        
        // Aplicar paginación
        var paginatedFollowing = await followingQuery.ToPaginatedListAsync(pageIndex, pageSize);
        
        if (!paginatedFollowing.Items.Any())
        {
            return Ok(new 
            { 
                data = new List<object>(),
                pageIndex = paginatedFollowing.PageIndex,
                pageSize = paginatedFollowing.PageSize,
                totalCount = paginatedFollowing.TotalCount,
                totalPages = paginatedFollowing.TotalPages,
                hasPreviousPage = paginatedFollowing.HasPreviousPage,
                hasNextPage = paginatedFollowing.HasNextPage
            });
        }
        
        // Extraer IDs de los usuarios seguidos para obtener sus datos en una sola consulta
        var followedIds = paginatedFollowing.Items.Select(f => f.FollowedId).ToList();
        
        // Cargar todos los usuarios en una sola consulta
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => followedIds.Contains(u.Id))
            .Select(u => new PublicUserRelationDTO
            {
                Id = u.Id,
                Avatar = u.Avatar,
                FirstName = u.FirstName,
                LastName = u.LastName,
                User1 = u.User1
            })
            .ToDictionaryAsync(u => u.Id);
        
        // Combinar datos de usuarios seguidos con sus fechas de seguimiento
        var followingData = paginatedFollowing.Items
            .Where(f => users.ContainsKey(f.FollowedId))
            .Select(f => new
            {
                Following = users[f.FollowedId],
                FollowDate = f.FollowDate
            })
            .ToList();
        
        return Ok(new 
        { 
            data = followingData,
            pageIndex = paginatedFollowing.PageIndex,
            pageSize = paginatedFollowing.PageSize,
            totalCount = paginatedFollowing.TotalCount,
            totalPages = paginatedFollowing.TotalPages,
            hasPreviousPage = paginatedFollowing.HasPreviousPage,
            hasNextPage = paginatedFollowing.HasNextPage
        });
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
    public async Task<IActionResult> GetUserLikedPosts(int id, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var userExists = await _context.Users.AsNoTracking().AnyAsync(u => u.Id == id);
        if (!userExists)
        {
            return NotFound(new { message = "Usuario no encontrado" });
        }

        // Consulta optimizada para obtener los likes paginados
        var likedPostsQuery = _context.LikePosts
            .AsNoTracking()
            .Where(lp => lp.UserId == id)
            .OrderByDescending(lp => lp.CreatedDate)
            .Select(lp => new { LikePost = lp, PostId = lp.PostId });

        var paginatedLikes = await likedPostsQuery.ToPaginatedListAsync(pageIndex, pageSize);
        
        // Extraer los IDs de los posts para hacer una sola consulta
        var postIds = paginatedLikes.Items.Select(lp => lp.PostId).ToList();
        
        // Cargar todos los posts en una sola consulta
        var posts = await _context.Posts
            .AsNoTracking()
            .Where(p => postIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);
        
        // Mapear los resultados
        var likedPostsData = paginatedLikes.Items
            .Where(lp => posts.ContainsKey(lp.PostId))
            .Select(lp => 
            {
                var post = posts[lp.PostId];
                return new
                {
                    Post = new 
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
                    LikedDate = lp.LikePost.CreatedDate
                };
            }).ToList();

        return Ok(new 
        { 
            data = likedPostsData,
            pageIndex = paginatedLikes.PageIndex,
            pageSize = paginatedLikes.PageSize,
            totalCount = paginatedLikes.TotalCount,
            totalPages = paginatedLikes.TotalPages,
            hasPreviousPage = paginatedLikes.HasPreviousPage,
            hasNextPage = paginatedLikes.HasNextPage
        });
    }
  }
}
