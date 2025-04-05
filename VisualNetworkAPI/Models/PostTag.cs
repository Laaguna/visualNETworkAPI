using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class PostTag
{
    public int PostId { get; set; }

    public int TagId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual Tag Tag { get; set; } = null!;
}
