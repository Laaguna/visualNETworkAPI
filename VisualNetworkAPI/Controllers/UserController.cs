using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualNetworkAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace VisualNetworkAPI.Controllers
{
  [Route("api/[controller]")]
  [Authorize]
  [ApiController]
  public class UserController : ControllerBase
  {
    private readonly VisualNetworkContext _context;
    public UserController(VisualNetworkContext context)
    {
      _context = context;
    }

    [HttpGet]
    [Route("users")]
    public async Task<IActionResult> GetAllUsers()
    {
      var users = await _context.Users.ToListAsync();
      var publicUsers = users.Select(u => new UserPublicDto
      {
        Id = u.Id,
        User = u.User1,
        Email = u.Email,
        FirstName = u.FirstName,
        LastName = u.LastName,
        DateBirth = u.DateBirth,
        Active = u.Active,
        Phone = u.Phone,
        Address = u.Address,
        Genre = u.Genre,
        CreatedBy = u.CreatedBy,
        CreatedDate = u.CreatedDate,
        LastUpdate = u.LastUpdate

      }).ToList();

      return StatusCode(StatusCodes.Status200OK, new { data = publicUsers });
    }

    [HttpGet]
    [Route("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
      var user = await _context.Users.FindAsync(id);

      if (user == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var publicUser = new UserPublicDto
      {
        Id = user.Id,
        User = user.User1,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        DateBirth = user.DateBirth,
        Active = user.Active,
        Phone = user.Phone,
        Address = user.Address,
        Genre = user.Genre,
        CreatedBy = user.CreatedBy,
        CreatedDate = user.CreatedDate,
        LastUpdate = user.LastUpdate
      };

      return Ok(new { data = publicUser });
    }

    [HttpGet]
    [Route("users/{id}/followers")]
    public async Task<IActionResult> GetUserFollowers(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var followerIds = await _context.Followers
          .Where(f => f.FollowedId == id)
          .Select(f => f.FollowerId)
          .ToListAsync();

      var followersData = new List<object>();
      foreach (var followerId in followerIds)
      {
        var follower = await _context.Users.FindAsync(followerId);
        if (follower != null)
        {
          followersData.Add(new
          {
            Follower = new UserPublicDto
            {
              Id = follower.Id,
              User = follower.User1,
              Email = follower.Email,
              FirstName = follower.FirstName,
              LastName = follower.LastName,
              DateBirth = follower.DateBirth,
              Active = follower.Active,
              Phone = follower.Phone,
              Address = follower.Address,
              Genre = follower.Genre,
              CreatedBy = follower.CreatedBy,
              CreatedDate = follower.CreatedDate,
              LastUpdate = follower.LastUpdate
            },
            FollowDate = await _context.Followers
                  .Where(f => f.FollowerId == followerId && f.FollowedId == id)
                  .Select(f => f.FollowDate)
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = followersData });
    }

    [HttpGet]
    [Route("users/{id}/following")]
    public async Task<IActionResult> GetUserFollowing(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var followingIds = await _context.Followers
          .Where(f => f.FollowerId == id)
          .Select(f => f.FollowedId)
          .ToListAsync();

      var followingData = new List<object>();
      foreach (var followingId in followingIds)
      {
        var followedUser = await _context.Users.FindAsync(followingId);
        if (followedUser != null)
        {
          followingData.Add(new
          {
            Following = new UserPublicDto
            {
              Id = followedUser.Id,
              User = followedUser.User1,
              Email = followedUser.Email,
              FirstName = followedUser.FirstName,
              LastName = followedUser.LastName,
              DateBirth = followedUser.DateBirth,
              Active = followedUser.Active,
              Phone = followedUser.Phone,
              Address = followedUser.Address,
              Genre = followedUser.Genre,
              CreatedBy = followedUser.CreatedBy,
              CreatedDate = followedUser.CreatedDate,
              LastUpdate = followedUser.LastUpdate
            },
            FollowDate = await _context.Followers
                  .Where(f => f.FollowerId == id && f.FollowedId == followingId)
                  .Select(f => f.FollowDate)
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = followingData });
    }

    [HttpGet]
    [Route("users/{id}/liked-posts")]
    public async Task<IActionResult> GetUserLikedPosts(int id)
    {
      var userExists = await _context.Users.AnyAsync(u => u.Id == id);
      if (!userExists)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      var likedPostIds = await _context.LikePosts
          .Where(lp => lp.UserId == id)
          .Select(lp => lp.PostId)
          .ToListAsync();

      var likedPostsData = new List<object>();
      foreach (var postId in likedPostIds)
      {
        var post = await _context.Posts.FindAsync(postId); 
        if (post != null)
        {
          likedPostsData.Add(new
          {
            Post = new Post
            {
              Id = post.Id,
              Title = post.Title,
              Description = post.Description,
              PostTags = post.PostTags,
              ImageUrls = post.ImageUrls,
              JsonPersonalizacion = post.JsonPersonalizacion,
              CreatedBy = post.CreatedBy,
              CreatedDate = post.CreatedDate,
              LastUpdate = post.LastUpdate
            },
            LikedDate = await _context.LikePosts
                  .Where(lp => lp.UserId == id && lp.PostId == postId)
                  .Select(lp => lp.CreatedDate) 
                  .FirstOrDefaultAsync()
          });
        }
      }

      return Ok(new { data = likedPostsData });
    }

    [HttpPut]
    [Route("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
    {
      if (!ModelState.IsValid)
      {
        return BadRequest(ModelState);
      }

      var userToUpdate = await _context.Users.FindAsync(id);

      if (userToUpdate == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      userToUpdate.FirstName = user.FirstName ?? userToUpdate.FirstName;
      userToUpdate.LastName = user.LastName ?? userToUpdate.LastName;
      userToUpdate.Email = user.Email ?? userToUpdate.Email;
      userToUpdate.Phone = user.Phone ?? userToUpdate.Phone;
      userToUpdate.Address = user.Address ?? userToUpdate.Address;
      userToUpdate.Genre = user.Genre ?? userToUpdate.Genre;
      userToUpdate.LastUpdate = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      var publicUser = new UserPublicDto
      {
        Id = userToUpdate.Id,
        User = userToUpdate.User1,
        Email = userToUpdate.Email,
        FirstName = userToUpdate.FirstName,
        LastName = userToUpdate.LastName,
        DateBirth = userToUpdate.DateBirth,
        Active = userToUpdate.Active,
        Phone = userToUpdate.Phone,
        Address = userToUpdate.Address,
        Genre = userToUpdate.Genre,
        CreatedBy = userToUpdate.CreatedBy,
        CreatedDate = userToUpdate.CreatedDate,
        LastUpdate = userToUpdate.LastUpdate
      };

      return Ok(new { data = publicUser });
    }

    [HttpDelete]
    [Route("users/{id}")]
    public async Task<IActionResult> DeactivateUser(int id)
    {
      var userToDeactivate = await _context.Users.FindAsync(id);

      if (userToDeactivate == null)
      {
        return NotFound(new { message = "Usuario no encontrado" });
      }

      if (!userToDeactivate.Active)
      {
        return BadRequest(new { message = "El usuario ya está inactivo" });
      }

      userToDeactivate.Active = false;
      userToDeactivate.LastUpdate = DateTime.UtcNow;

      await _context.SaveChangesAsync();

      return Ok(new { message = $"Usuario con ID {id} inactivado exitosamente" });
    }


  }
}
