using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Paginated;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class TagController : BaseUserController
  {

    private readonly VisualNetworkContext _context;
    public TagController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllTags([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var paginatedTags = await _context.Tags
            .AsNoTracking()
            .OrderBy(t => t.Title) // Ordenar alfabéticamente 
            .ToPaginatedListAsync(pageIndex, pageSize);

        return Ok(new 
        { 
            data = paginatedTags.Items,
            pageIndex = paginatedTags.PageIndex,
            pageSize = paginatedTags.PageSize,
            totalCount = paginatedTags.TotalCount,
            totalPages = paginatedTags.TotalPages,
            hasPreviousPage = paginatedTags.HasPreviousPage,
            hasNextPage = paginatedTags.HasNextPage
        });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTagById(int id)
    {
      var tag = await _context.Tags.FindAsync(id);
      if (tag == null)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }
      return Ok(new { data = tag });
    }

    [HttpPost]
    public async Task<IActionResult> CreateTag([FromBody] Tag tag)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }
      tag.CreatedBy = GetLoggedInUsername();
      tag.CreatedDate = DateTime.UtcNow;
      tag.LastUpdate = DateTime.UtcNow;
      _context.Tags.Add(tag);
      await _context.SaveChangesAsync();
      return CreatedAtAction(nameof(GetTagById), new { id = tag.Id }, new { data = tag });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTag(int id, [FromBody] Tag updatedTag)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var tagToUpdate = await _context.Tags.FindAsync(id);
      if (tagToUpdate == null)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }

      // Solo permitir actualizar el título y la última fecha de actualización
      tagToUpdate.Title = updatedTag.Title;
      tagToUpdate.LastUpdate = DateTime.UtcNow;

      _context.Entry(tagToUpdate).State = EntityState.Modified;
      await _context.SaveChangesAsync();
      return Ok(new { data = tagToUpdate });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTag(int id)
    {
      var tagToDelete = await _context.Tags.FindAsync(id);
      if (tagToDelete == null)
      {
        return NotFound(new { message = "Tag no encontrado" });
      }
      _context.Tags.Remove(tagToDelete);
      await _context.SaveChangesAsync();
      return NoContent();
    }

  }
}
