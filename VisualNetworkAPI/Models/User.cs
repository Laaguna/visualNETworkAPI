using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class User
{
    public int Id { get; set; }

    public string User1 { get; set; } = null!;

    public required string Password { get; set; } = null!;

    public required string Email { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? DateBirth { get; set; }

    public bool Active { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? Genre { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual ICollection<Follower> FollowerFolloweds { get; set; } = new List<Follower>();

    public virtual ICollection<Follower> FollowerFollowerNavigations { get; set; } = new List<Follower>();

    public virtual ICollection<LikePost> LikePosts { get; set; } = new List<LikePost>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
