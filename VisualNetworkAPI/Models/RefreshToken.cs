using System;
using System.Collections.Generic;

namespace VisualNetworkAPI.Models;

public partial class RefreshToken
{
    public Guid Id { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? ReplacedByToken { get; set; }

    public string? SessionId { get; set; }

    public string? DeviceInfo { get; set; }

    public virtual User User { get; set; } = null!;
}
