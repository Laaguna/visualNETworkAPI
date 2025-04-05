using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class Tag
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual ICollection<PostTag> PostTags { get; set; } = new List<PostTag>();
}
