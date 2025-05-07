namespace VisualNetworkAPI.Models.DTOs
{
  public class PostBoardDTO
  {
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Decoration { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
  }
}
