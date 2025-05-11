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
  public class BoardPostController : BaseUserController
  {
    private readonly VisualNetworkContext _context;
    public BoardPostController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpPost("{boardId}/posts/{postId}")] 
    public async Task<IActionResult> AddPostToBoard(int boardId, int postId)
    {
      var boardExists = await _context.Boards.AnyAsync(b => b.Id == boardId);
      if (!boardExists)
      {
        return NotFound(new { message = "Tablero no encontrado" });
      }

      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      // Check if the association already exists
      var existingBoardPost = await _context.BoardPosts
          .FirstOrDefaultAsync(bp => bp.BoardId == boardId && bp.PostId == postId);
      if (existingBoardPost != null)
      {
        return Conflict(new { message = "La publicación ya está en este tablero" });
      }

      var newBoardPost = new BoardPost
      {
        BoardId = boardId,
        PostId = postId,
        CreatedDate = DateTime.UtcNow
      };

      _context.BoardPosts.Add(newBoardPost);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetBoardPosts), new { boardId = boardId }, new { message = "Publicación agregada al tablero exitosamente" });
    }

    [HttpGet("{boardId}/posts")]  
    public async Task<IActionResult> GetBoardPosts(int boardId, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var boardExists = await _context.Boards.AsNoTracking().AnyAsync(b => b.Id == boardId);
        if (!boardExists)
        {
            return NotFound(new { message = "Tablero no encontrado" });
        }

        var paginatedBoardPosts = await _context.BoardPosts
            .AsNoTracking()
            .Where(bp => bp.BoardId == boardId)
            .OrderByDescending(bp => bp.CreatedDate) // Orden cronológico inverso
            .Select(bp => new
            {
                PostId = bp.PostId,
                PostTitle = bp.Post.Title,
                PostDescription = bp.Post.Description,
                PostImageUrl = bp.Post.ImageUrls,
                CreatedDate = bp.CreatedDate
            })
            .ToPaginatedListAsync(pageIndex, pageSize);

        return Ok(new 
        { 
            data = paginatedBoardPosts.Items,
            pageIndex = paginatedBoardPosts.PageIndex,
            pageSize = paginatedBoardPosts.PageSize,
            totalCount = paginatedBoardPosts.TotalCount,
            totalPages = paginatedBoardPosts.TotalPages,
            hasPreviousPage = paginatedBoardPosts.HasPreviousPage,
            hasNextPage = paginatedBoardPosts.HasNextPage
        });
    }

    [HttpDelete("{boardId}/posts/{postId}")] 
    public async Task<IActionResult> RemovePostFromBoard(int boardId, int postId)
    {
      var boardExists = await _context.Boards.AnyAsync(b => b.Id == boardId);
      if (!boardExists)
      {
        return NotFound(new { message = "Tablero no encontrado" });
      }

      var postExists = await _context.Posts.AnyAsync(p => p.Id == postId);
      if (!postExists)
      {
        return NotFound(new { message = "Publicación no encontrada" });
      }

      var boardPostToDelete = await _context.BoardPosts
          .FirstOrDefaultAsync(bp => bp.BoardId == boardId && bp.PostId == postId);

      if (boardPostToDelete == null)
      {
        return NotFound(new { message = "La publicación no está en este tablero" });
      }

      _context.BoardPosts.Remove(boardPostToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }

  }
}
