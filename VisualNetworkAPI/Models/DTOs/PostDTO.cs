namespace VisualNetworkAPI.Models.DTOs
{
  public class PostDTO
  {
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonPersonalizacion { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }
    public string? ImageUrls { get; set; }
    public int CommentCount { get; set; }
    public int LikeCount { get; set; }
    public int TagCount { get; set; }
  }
}
