using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using VisualNetworkAPI.Models;
using VisualNetworkAPI.Models.DTOs.Comments;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]

  public class PostController : ControllerBase
  {
    private readonly VisualNetworkContext _context;

    public PostController(VisualNetworkContext context)
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
    public async Task<IActionResult> CreatPost([FromBody] Post post)
    {
      if(!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      post.CreatedDate = DateTime.Now;
      post.LastUpdate = DateTime.Now;
      _context.Posts.Add(post);
      await _context.SaveChangesAsync();

      return CreatedAtAction(nameof(Post), new { id = post.Id}, new { data = post });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] Post post)
    {
      if (!ModelState.IsValid) return BadRequest(ModelState);

      if (id != post.Id) return BadRequest(new { message = "El ID del post no coincide con el ID proporcionado en la ruta" });

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
      if (!postExists)return NotFound(new { message = "Publicación no encontrada" });
      

      var newComment = new Comment
      {
        PostId = postId,
        Description = commentDto.Description,
        //CreatedBy = GetLoggedInUsername(), 
        CreatedDate = DateTime.UtcNow,
        LastUpdate = DateTime.UtcNow
      };

      _context.Comments.Add(newComment);
      await _context.SaveChangesAsync();

      var createdCommentDto = new GetCommentDTO
      {
        Id = newComment.Id,
        Description = newComment.Description,
        //CreatedBy = newComment.CreatedBy,
        CreatedDate = newComment.CreatedDate,
        LastUpdate = newComment.LastUpdate
      };

      return CreatedAtAction(nameof(GetAllPostComments), new { postId = postId }, new { data = createdCommentDto });
    }




    // TODO: --Reactions
    // GetAllPostReactions
    // CreatePostReaction
    // DeletePostReaction

    // TODO: Tags
    // TODO: DTOs CreatePostTag, GetPostTags
    // GetAllPostTags
    // CreatePostTag
    // DeletePostTag

    //TODO: Boards
    // GetAllBoards of a  post


  }
}
