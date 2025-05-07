using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using Microsoft.AspNetCore.Authorization;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Comments;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class CommentsController : BaseUserController
  {
    private readonly VisualNetworkContext _context;
    public CommentsController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpGet("{commentId}")]
    public async Task<IActionResult> GetCommentById(int commentId)
    {
      var comment = await _context.Comments.FindAsync(commentId);
      if (comment == null)
      {
        return NotFound(new { message = "Comentario no encontrado" });
      }

      var commentDto = new GetCommentDTO
      {
        Id = comment.Id,
        Description = comment.Description,
        CreatedDate = comment.CreatedDate,
        LastUpdate = comment.LastUpdate
      };

      var user = await _context.Users.FindAsync(int.Parse(comment.CreatedBy));
      if (user != null)
      {
        commentDto.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return Ok(new { data = commentDto });
    }

    [HttpPut("{commentId}")]
    public async Task<IActionResult> UpdateComment(int commentId, [FromBody] CreatePostCommentDTO commentDto)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var commentToUpdate = await _context.Comments.FindAsync(commentId);
      if (commentToUpdate == null)
      {
        return NotFound(new { message = "Comentario no encontrado" });
      }

      // Authorization check: Only the creator can update
      if (commentToUpdate.CreatedBy != GetLoggedInUsername())
      {
        return Unauthorized(new { message = "No tienes permiso para editar este comentario." });
      }

      commentToUpdate.Description = commentDto.Description;
      commentToUpdate.LastUpdate = DateTime.UtcNow;

      _context.Entry(commentToUpdate).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      var updatedCommentDto = new GetCommentDTO
      {
        Id = commentToUpdate.Id,
        Description = commentToUpdate.Description,
        CreatedDate = commentToUpdate.CreatedDate,
        LastUpdate = commentToUpdate.LastUpdate
      };
      return Ok(new { data = updatedCommentDto });
    }

    [HttpDelete("{commentId}")]
    public async Task<IActionResult> DeleteComment(int commentId)
    {
      var commentToDelete = await _context.Comments.FindAsync(commentId);
      if (commentToDelete == null)
      {
        return NotFound(new { message = "Comentario no encontrado" });
      }

      // Authorization check: Only the creator can delete
      if (commentToDelete.CreatedBy != GetLoggedInUsername())
      {
        return Unauthorized(new { message = "No tienes permiso para borrar este comentario." });
      }

      _context.Comments.Remove(commentToDelete);
      await _context.SaveChangesAsync();

      return NoContent();
    }


  }
}
