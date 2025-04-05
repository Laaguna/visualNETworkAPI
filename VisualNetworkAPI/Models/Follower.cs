using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class Follower
{
    public int FollowerId { get; set; }

    public int FollowedId { get; set; }

    public DateTime? FollowDate { get; set; }

    public virtual User Followed { get; set; } = null!;

    public virtual User FollowerNavigation { get; set; } = null!;
}
