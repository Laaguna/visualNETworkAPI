namespace VisualNetworkAPI.Models.DTOs
{
  public class PublicPostDTO
  {
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? JsonPersonalizacion { get; set; }
    public List<string>? ImageUrls { get; set; }
    public PublicUserRelationDTO? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }
  }
}

