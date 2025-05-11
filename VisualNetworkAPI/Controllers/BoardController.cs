using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs;
using VisualNetworkAPI.Models.DTOs.Board;
using VisualNetworkAPI.Paginated;

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
  public async Task<IActionResult> GetAllBoards([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
  {
      // Aplicar paginación a nivel de consulta con AsNoTracking para mejorar rendimiento
      var paginatedBoards = await _context.Boards
          .AsNoTracking()
          .OrderByDescending(b => b.CreatedDate) // Ordenar por fecha de creación (ajusta según necesites)
          .ToPaginatedListAsync(pageIndex, pageSize);
      
      var boardsDTO = new List<PublicBoardDTO>();
      
      // Optimización: Obtener todos los userIds de una vez para hacer una sola consulta
      var userIds = paginatedBoards.Items
          .Select(b => int.Parse(b.CreatedBy))
          .Distinct()
          .ToList();
      
      // Cargar todos los usuarios relacionados en una sola consulta
      var users = await _context.Users
          .AsNoTracking()
          .Where(u => userIds.Contains(u.Id))
          .ToDictionaryAsync(u => u.Id);
      
      foreach (var board in paginatedBoards.Items)
      {
          var boardDTO = new PublicBoardDTO
          {
              Id = board.Id,
              Decoration = board.Decoration,
              Description = board.Description,
              CreatedDate = board.CreatedDate,
              LastUpdate = board.LastUpdate
          };

          // Usar el diccionario de usuarios en lugar de consultar cada uno
          if (int.TryParse(board.CreatedBy, out var userId) && users.TryGetValue(userId, out var user))
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

      // Retornar datos paginados con metadatos de paginación
      return Ok(new 
      { 
          success = true,
          data = boardsDTO,
          pageIndex = paginatedBoards.PageIndex,
          pageSize = paginatedBoards.PageSize,
          totalCount = paginatedBoards.TotalCount,
          totalPages = paginatedBoards.TotalPages,
          hasPreviousPage = paginatedBoards.HasPreviousPage,
          hasNextPage = paginatedBoards.HasNextPage
      });
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
