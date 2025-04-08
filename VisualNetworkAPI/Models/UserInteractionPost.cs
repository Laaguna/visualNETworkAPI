using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VisualNetworkAPI.Models;

public partial class UserPostInteraction
{
  [Key]
  [Column(Order = 0)]
  public int UserId { get; set; }

  [Key]
  [Column(Order = 1)]
  public int PostId { get; set; }

  [Key]
  [Column(Order = 2)]
  [StringLength(50)]
  public string InteractionType { get; set; } = null!;

  public DateTime? InteractionDate { get; set; }

  [StringLength(50)]
  public string? CreatedBy { get; set; }

  public DateTime? CreatedDate { get; set; }

  public DateTime? LastUpdate { get; set; }

  [ForeignKey(nameof(PostId))]
  [InverseProperty("UserPostInteractions")]
  public virtual Post Post { get; set; } = null!;

  [ForeignKey(nameof(UserId))]
  [InverseProperty("UserPostInteractions")]
  public virtual User User { get; set; } = null!;
}
