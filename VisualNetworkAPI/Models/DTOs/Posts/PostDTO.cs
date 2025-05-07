namespace VisualNetworkAPI.Models.DTOs
{
  public class PostDTO
  {
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonPersonalizacion { get; set; }
    public List<IFormFile>? Images { get; set; }
    public DateTime? CreatedDate { get; set; }
  }
}
