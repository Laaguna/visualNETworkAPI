using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;

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
      return Ok(new { data = boards }); 
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetBoardById(int id)
    {
      var board = await _context.Boards.FindAsync(id);

      if (board == null)
      {
        return NotFound(new { message = "Tablero no encontrado" });
      }

      return Ok(new { data = board }); 
    }

    [HttpPost]
    public async Task<IActionResult> CreateBoard([FromBody] Board board)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      board.CreatedBy = GetLoggedInUsername();
      board.CreatedDate = DateTime.UtcNow;
      board.LastUpdate = DateTime.UtcNow;
      _context.Boards.Add(board);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(GetBoardById), new { id = board.Id }, new { data = board }); // Considera usar un DTO para la respuesta
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
