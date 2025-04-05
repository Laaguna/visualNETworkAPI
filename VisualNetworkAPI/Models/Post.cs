using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class Post
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? JsonPersonalizacion { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public string? ImageUrls { get; set; }

    public virtual ICollection<BoardPost> BoardPosts { get; set; } = new List<BoardPost>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual User? CreatedByNavigation { get; set; }

    public virtual ICollection<LikePost> LikePosts { get; set; } = new List<LikePost>();

    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
