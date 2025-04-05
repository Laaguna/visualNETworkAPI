using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class BoardPost
{
    public int BoardId { get; set; }

    public int PostId { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

    public virtual Board Board { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
