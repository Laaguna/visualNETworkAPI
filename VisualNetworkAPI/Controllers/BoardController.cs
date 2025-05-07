using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Board;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class BoardController : BaseUserController
  {

    private readonly VisualNetworkContext _context;
    public BoardController(VisualNetworkContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoards()
    {
      var boards = await _context.Boards.ToListAsync();
      var boardsDTO = new List<PublicBoardDTO>();

      foreach (var board in boards)
      {
        var boardDTO = new PublicBoardDTO
        {
          Id = board.Id,
          Decoration = board.Decoration,
          Description = board.Description,
          CreatedDate = board.CreatedDate,
          LastUpdate = board.LastUpdate
        };

        var user = await _context.Users.FindAsync(int.Parse(board.CreatedBy));
        if (user != null)
        {
          boardDTO.CreatedBy = new PublicUserRelationDTO
          {
            Id = user.Id,
            User1 = user.User1,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Avatar = user.Avatar,
          };
        }
        boardsDTO.Add(boardDTO);
    }

      return Ok(new { data = boardsDTO }); 
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoardById(int id)
    {
      var board = await _context.Boards.FindAsync(id);

      if (board == null) return NotFound(new { message = "Tablero no encontrado" });

      var boardDTO = new PublicBoardDTO
      {
        Id = board.Id,
        Description = board.Description,
        Decoration = board.Decoration,
        CreatedDate = board.CreatedDate,
        LastUpdate = board.LastUpdate
      };

      var user = await _context.Users.FindAsync(int.Parse(board.CreatedBy));

      if (user != null)
      {
        boardDTO.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return Ok(new { data = boardDTO }); 
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] PostBoardDTO board)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userId = GetLoggedInUserId();

      var boardToCreate = new Board
      {
        Description = board.Description,
        Decoration = board.Decoration,
        CreatedBy = userId.ToString(),
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow,
      };

      _context.Boards.Add(boardToCreate);
      await _context.SaveChangesAsync();

      var user = await _context.Users.FindAsync(userId);

      var publicBoardDTO = new PublicBoardDTO
      {
        Id = boardToCreate.Id,
        Decoration = boardToCreate.Decoration,
        Description = boardToCreate.Description,
        CreatedDate = boardToCreate.CreatedDate,
        LastUpdate = boardToCreate.LastUpdate
      };

      if (user != null)
      {
        publicBoardDTO.CreatedBy = new PublicUserRelationDTO
        {
          Id = user.Id,
          User1 = user.User1,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Avatar = user.Avatar,
        };
      }

      return CreatedAtAction(nameof(GetBoardById), new { id = publicBoardDTO.Id }, new { data = publicBoardDTO }); // Considera usar un DTO para la respuesta
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBoard(int id, [FromBody] Board board)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var boardToUpdate = await _context.Boards.FindAsync(id);

      if (boardToUpdate == null)
      {
        return NotFound(new { message = "Tablero no encontrado" });
      }

      boardToUpdate.Description = board.Description;
      boardToUpdate.Decoration = board.Decoration;
      boardToUpdate.LastUpdate = DateTime.UtcNow;

      _context.Entry(boardToUpdate).State = EntityState.Modified;
      await _context.SaveChangesAsync();

      return Ok(new { data = boardToUpdate });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBoard(int id)
    {
      var boardToDelete = await _context.Boards.FindAsync(id);

      if (boardToDelete == null)
      {
        return NotFound(new { message = "Tablero no encontrado" });
      }

      _context.Boards.Remove(boardToDelete);
      await _context.SaveChangesAsync();

      return NoContent(); // Devuelve 204 No Content para indicar éxito sin cuerpo de respuesta
    }

  }
}
