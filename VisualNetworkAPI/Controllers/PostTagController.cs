using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Paginated;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class PostTagController : BaseUserController
  {

    private readonly VisualNetworkContext _context;
    public PostTagController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpPost("/api/Post/{postId}/tags/{tagId}")] // Mantiene la ruta original
    public async Task<IActionResult> AddTagToPost(int postId, int tagId)
    {
      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
      if (!tagExists)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }

      // Check if the association already exists
      var existingPostTag = await _context.PostTags
          .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);
      if (existingPostTag != null)
      {
        return Conflict(new { message = "El tag ya está asignado a esta publicación" });
      }

      var newPostTag = new PostTag
      {
        PostId = postId,
        TagId = tagId,
        CreatedBy = GetLoggedInUsername(),
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow
      };

      _context.PostTags.Add(newPostTag);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetPostTags), new { postId = postId }, new { message = "Tag agregado a la publicación exitosamente" });
    }

    // GET: api/Post/{postId}/tags
    [HttpGet("/api/Post/{postId}/tags")]
    public async Task<IActionResult> GetPostTags(int postId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var postExists = await _context.Posts.AsNoTracking().AnyAsync(p => p.Id == postId);
        if (!postExists)
        {
            return NotFound(new { message = "Publicación no encontrada" });
        }

        var paginatedPostTags = await _context.PostTags
            .AsNoTracking()
            .Where(pt => pt.PostId == postId)
            .OrderBy(pt => pt.Tag.Title) // Ordenar alfabéticamente por título de etiqueta
            .Select(pt => new
            {
                TagId = pt.TagId,
                TagTitle = pt.Tag.Title,
                CreatedBy = pt.CreatedBy,
                CreatedDate = pt.CreatedDate,
                LastUpdate = pt.LastUpdate
            })
            .ToPaginatedListAsync(pageIndex, pageSize);

        return Ok(new 
        { 
            data = paginatedPostTags.Items,
            pageIndex = paginatedPostTags.PageIndex,
            pageSize = paginatedPostTags.PageSize,
            totalCount = paginatedPostTags.TotalCount,
            totalPages = paginatedPostTags.TotalPages,
            hasPreviousPage = paginatedPostTags.HasPreviousPage,
            hasNextPage = paginatedPostTags.HasNextPage
        });
    }

    // GET: api/Tag/{tagId}/posts
    [HttpGet("/api/Tag/{tagId}/posts")] // Mantiene la ruta original
    public async Task<IActionResult> GetTagPosts(int tagId)
    {
      var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
      if (!tagExists)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }

      var tagPosts = await _context.PostTags
          .Where(pt => pt.TagId == tagId)
          .Include(pt => pt.Post) // Include Post data
          .Select(pt => new
          {
            PostId = pt.PostId,
            PostTitle = pt.Post.Title, // Access Post properties
            PostDescription = pt.Post.Description,
            PostImageUrl = pt.Post.ImageUrls,
            CreatedBy = pt.CreatedBy,
            CreatedDate = pt.CreatedDate,
            LastUpdate = pt.LastUpdate
          })
          .ToListAsync();

      return Ok(new { data = tagPosts });
    }

    // DELETE: api/Post/{postId}/tags/{tagId}
    [HttpDelete("/api/Post/{postId}/tags/{tagId}")] // Mantiene la ruta original
    public async Task<IActionResult> RemoveTagFromPost(int postId, int tagId)
    {
      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var tagExists = await _context.Tags.AnyAsync(t => t.Id == tagId);
      if (!tagExists)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }

      var postTagToDelete = await _context.PostTags
          .FirstOrDefaultAsync(pt => pt.PostId == postId && pt.TagId == tagId);

      if (postTagToDelete == null)
      {
        return NotFound(new { message = "El tag no está asignado a esta publicación" });
      }

      _context.PostTags.Remove(postTagToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }
  }
}
