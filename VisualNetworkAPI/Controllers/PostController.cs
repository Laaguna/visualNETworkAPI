using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Comments;

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

    // GetAllPosts
    // GetPostById
    // CreatePost
    // UpdatePost
    // DeletePost

    [HttpGet]
    public async Task<IActionResult> GetAllPosts()
    {
      var posts = await _context.Posts.ToListAsync();
      return Ok(new { data = posts });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostId(int id)
    {
      var post = await _context.Posts.FindAsync(id);

      if (post == null) return NotFound(new {message = "Post no encontrado"});
      return Ok(new { data = post });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePost([FromForm] PostDTO postDto)
    {
      if (postDto.Image == null || postDto.Image.Length == 0)
      {
        return BadRequest("Debe subir una imagen.");
      }

      // Generar nombre único para la imagen
      var fileName = $"{Guid.NewGuid()}{Path.GetExtension(postDto.Image.FileName)}";
      var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

      // Guardar imagen
      using (var stream = new FileStream(imagePath, FileMode.Create))
      {
        await postDto.Image.CopyToAsync(stream);
      }

      // Crear el post
      var post = new Post
      {
        Title = postDto.Title,
        Description = postDto.Description,
        JsonPersonalizacion = postDto.JsonPersonalizacion,
        CreatedBy = GetLoggedInUserId(),
        CreatedDate = DateTime.Now,
        LastUpdate = DateTime.Now,
        ImageUrls = $"/images/{fileName}"
      };

      _context.Posts.Add(post);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetPostId), new { id = post.Id }, new { data = post });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] Post post)
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);


      var postToUpdate = await _context.Posts.FindAsync(id);

      if (postToUpdate == null) return NotFound(new { message = "Post no encontrado " });

      postToUpdate.Description = post.Description;
      postToUpdate.Title = post.Title;
      postToUpdate.JsonPersonalizacion = post.JsonPersonalizacion;
      postToUpdate.ImageUrls = post.ImageUrls;
      postToUpdate.LastUpdate = DateTime.Now;

      _context.Entry(postToUpdate).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      return Ok(new { data = postToUpdate });

    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePost(int id)
    {
      var postToDelete = await _context.Posts.FindAsync(id);

      if (postToDelete == null) return NotFound(new { message = "Post no encontrado" });

      _context.Posts.Remove(postToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }

    // GetAllPostComments
    // CreatePostComment
    // UpdatePostComment
    // DeletePostComment

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
    public async Task<IActionResult> GetPostLikes(int postId)
    {
      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var likes = await _context.LikePosts
          .Where(lp => lp.PostId == postId)
          .Include(lp => lp.User) // Incluye la información del usuario que dio like
          .Select(lp => new
          {
            UserId = lp.UserId,
            UserName = lp.User.User1, // O las propiedades del usuario que quieras exponer
            LikedDate = lp.CreatedDate
            // Puedes agregar más propiedades del usuario si es necesario (nombre, etc.)
          })
          .ToListAsync();

      return Ok(new { data = likes });
    }


    // GET: api/Post/{postId}/comments
    [HttpGet("{postId}/comments")]
    public async Task<IActionResult> GetAllPostComments(int postId)
    {
      var postExist = await _context.Posts.FindAsync(postId);
      if (postExist == null) return NotFound(new { message = "Post no encontrado" });

      var comments = await _context.Comments
      .Where(c => c.PostId == postId)
                .Select(c => new GetCommentDTO
                {
                  Id = c.Id,
                  Description = c.Description,
                  CreatedBy = c.CreatedBy,
                  CreatedDate = c.CreatedDate,
                  LastUpdate = c.LastUpdate
                })
                .ToListAsync();

      return Ok(new { data = comments });
    }

    // POST: api/Post/{postId}/comments
    [HttpPost("{postId}/comments")]
    public async Task<IActionResult> CreatePostComment(int postId, [FromBody] CreatePostCommentDTO commentDto)
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists) return NotFound(new { message = "Publicación no encontrada" });


      var newComment = new Comment
      {
        PostId = postId,
        Description = commentDto.Description,
        CreatedBy = GetLoggedInUsername(),
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow
      };

      _context.Comments.Add(newComment);
      await _context.SaveChangesAsync();

      var createdCommentDto = new GetCommentDTO
      {
        Id = newComment.Id,
        Description = newComment.Description,
        CreatedBy = newComment.CreatedBy, // Include CreatedBy in the response
        CreatedDate = newComment.CreatedDate,
        LastUpdate = newComment.LastUpdate
      };

      return CreatedAtAction(nameof(GetAllPostComments), new { postId = postId }, new { data = createdCommentDto });
    }



    /* TODO: Access
    POST    /api/Access/register
    POST    /api/Access/login
    POST    /api/Access/refresh-token
    POST    /api/Access/logout
    */

    /* TODO: User
    GET     /api/User
    GET     /api/User/{id}
    PUT     /api/User/{id}
    DELETE  /api/User/{id}
    */

    /* TODO: Followers
    GET     /api/User/{id}/Followers
    GET     /api/User/{id}/Following
    // PENDING: READY
    POST    /api/User/{id}/Followers - R
    DELETE  /api/User/{id}/Followers/{followerId} - R
    */

    /* TODO: Like Post
    GET     /api/User/{id}/liked-posts
    // PENDING: READY
    POST    /api/Post/{postId}/likes - R
    DELETE  /api/Post/{postId}/likes/{userId} - R
    GET     /api/Post/{postId}/likes - R 
    */

    /* TODO: Post
    GET     /api/Post
    GET     /api/Post/{id}
    POST    /api/Post
    PUT     /api/Post/{id}
    DELETE  /api/Post/{id}
    // PENDING: READY
    GET     /api/User/{userId}/posts - Get posts of a user - R 
    */

    /* TODO: Comments
    GET     /api/Post/{postId}/comments
    PUT     /api/Post/{postId}/comments   // DELETE THIS EP, REPLACED BY PUT COMMENT
    // PENDING: READY
    POST    /api/Post/{postId}/comments - Crear un comment - R
    GET     /api/Comments/{commentId} - Obtener un comment - R
    PUT     /api/Comments/{commentId} - Editar un comment - R
    DELETE  /api/Comments/{commentId} - Borrar un comment - R
    */

    /* TODO: Board
    GET     /api/Board
    POST    /api/Board
    PUT     /api/Board/{id}
    DELETE  /api/Board/{id}
    */

    /* TODO: Board_Post
    // PENDING: READY
    POST    /api/Board/{boardId}/posts        // Add a post to a board - R
    GET     /api/Board/{boardId}/posts        // Get all posts on a specific board - R
    DELETE  /api/Board/{boardId}/posts/{postId} // Remove a post from a board - R
    */

    /* TODO: Tag
    // PENDING:
    POST    /api/Tag
    GET     /api/Tag
    GET     /api/Tag/{id}
    PUT     /api/Tag/{id}
    DELETE  /api/Tag/{id}
    */

    /* TODO: Post_Tag
    // PENDING:
    POST    /api/Post/{postId}/tags       // Add a tag to a post
    GET     /api/Post/{postId}/tags       // Get all tags for a post
    GET     /api/Tag/{tagId}/posts        // Get all posts for a specific tag
    DELETE  /api/Post/{postId}/tags/{tagId} // Remove a tag from a post
    */


  }
}
