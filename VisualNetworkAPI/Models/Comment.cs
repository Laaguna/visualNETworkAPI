using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class Comment
{
    public int Id { get; set; }

    public int PostId { get; set; }

    public string? Description { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual Post Post { get; set; } = null!;
}
