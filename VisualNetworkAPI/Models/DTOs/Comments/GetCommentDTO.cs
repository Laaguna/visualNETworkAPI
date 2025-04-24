namespace VisualNetworkAPI.Models.DTOs.Comments
{
  public class GetCommentDTO
  {
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? CreatedBy { get; set; } 
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }
  }
}
