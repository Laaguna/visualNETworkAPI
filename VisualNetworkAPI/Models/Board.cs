using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class Board
{
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? Decoration { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual ICollection<BoardPost> BoardPosts { get; set; } = new List<BoardPost>();
}
