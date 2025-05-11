using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Board;
using VisualNetworkAPI.Models.DTOs.Comments;
using VisualNetworkAPI.Paginated;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]

  public class PostController : BaseUserController
  {
    private readonly VisualNetworkContext _context;

    public PostController(VisualNetworkContext context) : base()
    {
      _context = context;
    }

    [HttpGet]
public async Task<IActionResult> GetAllPosts([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
{
    // Usar AsNoTracking() para mejorar el rendimiento cuando solo leemos datos
    var paginatedPosts = await _context.Posts
        .AsNoTracking()
        .OrderByDescending(p => p.CreatedDate) // Añadido ordenamiento por fecha
        .ToPaginatedListAsync(pageIndex, pageSize);
    
    var postDtos = new List<PublicPostDTO>();

      // Optimización: Obtener todos los userIds para hacer una única consulta
#pragma warning disable CS8629 // Un tipo que acepta valores NULL puede ser nulo.
      var userIds = paginatedPosts.Items
        .Where(p => p.CreatedBy.HasValue)
        .Select(p => p.CreatedBy.Value)
        .Distinct()
        .ToList();
#pragma warning restore CS8629 // Un tipo que acepta valores NULL puede ser nulo.

      // Cargar todos los usuarios relacionados en una sola consulta
      var users = await _context.Users
        .AsNoTracking()
        .Where(u => userIds.Contains(u.Id))
        .ToDictionaryAsync(u => u.Id);
    
    foreach (var post in paginatedPosts.Items)
    {
        var postDto = new PublicPostDTO
        {
            Id = post.Id,
            Title = post.Title,
            Description = post.Description,
            JsonPersonalizacion = post.JsonPersonalizacion,
            ImageUrls = !string.IsNullOrEmpty(post.ImageUrls) ? post.ImageUrls.Split(',').ToList() : null,
            CreatedDate = post.CreatedDate,
            LastUpdate = post.LastUpdate
        };

        if (post.CreatedBy.HasValue && users.TryGetValue(post.CreatedBy.Value, out var user))
        {
            postDto.CreatedBy = new PublicUserRelationDTO
            {
                Id = user.Id,
                User1 = user.User1,
                FirstName = user.FirstName,
                Avatar = user.Avatar,
                LastName = user.LastName
            };
        }

        postDtos.Add(postDto);
    }

    return Ok(new 
    { 
        success = true,
        data = postDtos,
        pageIndex = paginatedPosts.PageIndex,
        pageSize = paginatedPosts.PageSize,
        totalCount = paginatedPosts.TotalCount,
        totalPages = paginatedPosts.TotalPages,
        hasPreviousPage = paginatedPosts.HasPreviousPage,
        hasNextPage = paginatedPosts.HasNextPage
    });
}

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostId(int id)
    {
      var post = await _context.Posts.FindAsync(id);
      if (post == null)
        return NotFound(new { message = "Post no encontrado" });

      var postDto = new PublicPostDTO
      {
        Id = post.Id,
        Title = post.Title,
        Description = post.Description,
        JsonPersonalizacion = post.JsonPersonalizacion,
        ImageUrls = !string.IsNullOrEmpty(post.ImageUrls) ? post.ImageUrls.Split(',').ToList() : null,
        CreatedDate = post.CreatedDate,
        LastUpdate = post.LastUpdate
      };

      if (post.CreatedBy.HasValue)
      {
        var user = await _context.Users.FindAsync(post.CreatedBy.Value);

        if (user != null)
        {
          postDto.CreatedBy = new PublicUserRelationDTO
          {
            Id = user.Id,
            User1 = user.User1,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar,
          };
        }
      }

      return Ok(new { data = postDto });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] PostDTO postDto)
    {
      if (postDto.Images == null || !postDto.Images.Any())
      {
        return BadRequest("Debe subir al menos una imagen.");
      }

      var imageUrls = new List<string>();
      foreach (var image in postDto.Images)
      {
        if (image.Length == 0) continue;
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
        var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);
        using (var stream = new FileStream(imagePath, FileMode.Create))
        {
          await image.CopyToAsync(stream);
        }
        imageUrls.Add($"/images/{fileName}");
      }

      var userId = GetLoggedInUserId();

      var post = new Post
      {
        Title = postDto.Title,
        Description = postDto.Description,
        JsonPersonalizacion = postDto.JsonPersonalizacion,
        CreatedBy = userId,
        CreatedDate = DateTime.Now,
        LastUpdate = DateTime.Now,
        ImageUrls = string.Join(",", imageUrls)
      };

      _context.Posts.Add(post);
      await _context.SaveChangesAsync();

      var user = await _context.Users.FindAsync(userId);

      var publicPostDto = new PublicPostDTO
      {
        Id = post.Id,
        Title = post.Title,
        Description = post.Description,
        JsonPersonalizacion = post.JsonPersonalizacion,
        ImageUrls = imageUrls,
        CreatedDate = post.CreatedDate,
        LastUpdate = post.LastUpdate
      };

      if (user != null)
      {
        publicPostDto.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return CreatedAtAction(nameof(GetPostId), new { id = post.Id }, new { data = publicPostDto });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDTO updateDto)
    {
      if (!ModelState.IsValid)
        return BadRequest(ModelState);

      var postToUpdate = await _context.Posts.FindAsync(id);
      if (postToUpdate == null)
        return NotFound(new { message = "Post no encontrado" });

      // Verificar que el usuario actual sea el creador del post
      var currentUserId = GetLoggedInUserId();
      if (postToUpdate.CreatedBy != currentUserId)
        return Forbid(new { message = "No tienes permiso para editar este post" }.ToString());

      postToUpdate.Description = updateDto.Description;
      postToUpdate.Title = updateDto.Title;
      postToUpdate.JsonPersonalizacion = updateDto.JsonPersonalizacion;

      postToUpdate.LastUpdate = DateTime.Now;
      postToUpdate.ImageUrls = postToUpdate.ImageUrls;

      _context.Entry(postToUpdate).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      var user = await _context.Users.FindAsync(postToUpdate.CreatedBy);

      var publicPostDto = new PublicPostDTO
      {
        Id = postToUpdate.Id,
        Title = postToUpdate.Title,
        Description = postToUpdate.Description,
        JsonPersonalizacion = postToUpdate.JsonPersonalizacion,
        
        CreatedDate = postToUpdate.CreatedDate,
        LastUpdate = postToUpdate.LastUpdate
      };

      if (user != null)
      {
        publicPostDto.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return Ok(new { data = publicPostDto });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
      var postToDelete = await _context.Posts.FindAsync(id);

      if (postToDelete == null) return NotFound(new { message = "Post no encontrado" });

      var currentUserId = GetLoggedInUserId();
      if (postToDelete.CreatedBy != currentUserId)
        return Forbid(new { message = "No tienes permiso para editar este post" }.ToString());


      _context.Posts.Remove(postToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }



    [HttpPost("{postId}/likes")]
    public async Task<IActionResult> LikePost(int postId)
    {
      var postToLike = await _context.Posts.FindAsync(postId);
      if (postToLike == null)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var currentUserId = GetLoggedInUserId();
      if (!currentUserId.HasValue)
      {
        return Unauthorized(new { message = "Usuario no autenticado" });
      }

      // Verifica si el usuario ya le dio like al post
      var existingLike = await _context.LikePosts
          .FirstOrDefaultAsync(lp => lp.PostId == postId && lp.UserId == currentUserId.Value);

      if (existingLike != null)
      {
        return Conflict(new { message = "Ya has dado \"Me Gusta\" a esta publicación" });
      }

      var newLike = new LikePost
      {
        PostId = postId,
        UserId = currentUserId.Value,
        CreatedDate = DateTime.UtcNow // Usa CreatedDate para almacenar la fecha del "like"
      };

      _context.LikePosts.Add(newLike);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetPostLikes), new { postId = postId }, new { message = "Te gusta esta publicación" });
    }

    [HttpDelete("{postId}/likes")]
    public async Task<IActionResult> UnlikePost(int postId)
    {
      var postToUnlike = await _context.Posts.FindAsync(postId);
      if (postToUnlike == null)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var currentUserId = GetLoggedInUserId();
      if (!currentUserId.HasValue)
      {
        return Unauthorized(new { message = "Usuario no autenticado" });
      }


      // Verifica si existe el "like"
      var likeToDelete = await _context.LikePosts
          .FirstOrDefaultAsync(lp => lp.PostId == postId && lp.UserId == currentUserId.Value);

      if (likeToDelete == null)
      {
        return NotFound(new { message = "No has dado \"Me Gusta\" a esta publicación" });
      }

      _context.LikePosts.Remove(likeToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    [HttpGet("{postId}/likes")]
    public async Task<IActionResult> GetPostLikes(int postId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var postExists = await _context.Posts.AsNoTracking().AnyAsync(p => p.Id == postId);
        if (!postExists)
        {
            return NotFound(new { message = "Publicación no encontrada" });
        }

        var paginatedLikes = await _context.LikePosts
            .AsNoTracking()
            .Where(lp => lp.PostId == postId)
            .OrderByDescending(lp => lp.CreatedDate)
            .Select(lp => new
            {
                UserId = lp.UserId,
                UserName = lp.User.User1,
                LikedDate = lp.CreatedDate
            })
            .ToPaginatedListAsync(pageIndex, pageSize);

        return Ok(new 
        { 
            data = paginatedLikes.Items,
            pageIndex = paginatedLikes.PageIndex,
            pageSize = paginatedLikes.PageSize,
            totalCount = paginatedLikes.TotalCount,
            totalPages = paginatedLikes.TotalPages,
            hasPreviousPage = paginatedLikes.HasPreviousPage,
            hasNextPage = paginatedLikes.HasNextPage
        });
}


    // GET: api/Post/{postId}/comments
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetAllPostComments(int postId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        // Verificar si el post existe
        var postExists = await _context.Posts.AsNoTracking().AnyAsync(p => p.Id == postId);
        if (!postExists)
            return NotFound(new { message = "Post no encontrado" });

        // Obtener comentarios paginados
        var paginatedComments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderByDescending(c => c.CreatedDate)
            .ToPaginatedListAsync(pageIndex, pageSize);

        if (!paginatedComments.Items.Any())
            return Ok(new 
            { 
                data = new List<GetCommentDTO>(),
                pageIndex = paginatedComments.PageIndex,
                pageSize = paginatedComments.PageSize,
                totalCount = paginatedComments.TotalCount,
                totalPages = paginatedComments.TotalPages,
                hasPreviousPage = paginatedComments.HasPreviousPage,
                hasNextPage = paginatedComments.HasNextPage
            });

        // Obtener todos los IDs de usuarios creadores como strings
        var creatorIds = paginatedComments.Items
            .Where(c => !string.IsNullOrEmpty(c.CreatedBy))
            .Select(c => c.CreatedBy)
            .Distinct()
            .ToList();

        // Cargar todos los usuarios creadores de una vez
        var userIds = creatorIds.Select(id => int.Parse(id)).ToList();
        var users = await _context.Users
            .AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id.ToString());

        // Crear los DTOs con la información de usuarios
        var commentsDTO = paginatedComments.Items.Select(c => {
            var commentDTO = new GetCommentDTO
            {
                Id = c.Id,
                Description = c.Description,
                CreatedDate = c.CreatedDate,
                LastUpdate = c.LastUpdate
            };

            if (!string.IsNullOrEmpty(c.CreatedBy) && users.TryGetValue(c.CreatedBy, out var user))
            {
                commentDTO.CreatedBy = new PublicUserRelationDTO
                {
                    Id = user.Id,
                    User1 = user.User1,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Avatar = user.Avatar,
                };
            }

            return commentDTO;
        }).ToList();

        return Ok(new 
        { 
            data = commentsDTO,
            pageIndex = paginatedComments.PageIndex,
            pageSize = paginatedComments.PageSize,
            totalCount = paginatedComments.TotalCount,
            totalPages = paginatedComments.TotalPages,
            hasPreviousPage = paginatedComments.HasPreviousPage,
            hasNextPage = paginatedComments.HasNextPage
        });
    }

    // POST: api/Post/{postId}/comments
    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> CreatePostComment(int postId, [FromBody] CreatePostCommentDTO commentDto)
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists) return NotFound(new { message = "Publicación no encontrada" });


      var userId = GetLoggedInUserId();

      var newComment = new Comment
      {
        PostId = postId,
        Description = commentDto.Description,
        CreatedBy = userId.ToString(),
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow
      };

      _context.Comments.Add(newComment);
      await _context.SaveChangesAsync();

      var createdCommentDto = new GetCommentDTO
      {
        Id = newComment.Id,
        Description = newComment.Description,
        CreatedDate = newComment.CreatedDate,
        LastUpdate = newComment.LastUpdate
      };

      var user = await _context.Users.FindAsync(userId);
      if (user != null)
      {
        createdCommentDto.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return CreatedAtAction(nameof(GetAllPostComments), new { postId = postId }, new { data = createdCommentDto });
    }

  }
}
