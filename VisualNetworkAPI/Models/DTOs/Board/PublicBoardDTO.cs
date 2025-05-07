namespace VisualNetworkAPI.Models.DTOs.Board
{
  public class PublicBoardDTO
  {
    public int Id { get; set; }

    public string? Description { get; set; }

    public string? Decoration { get; set; }

    public PublicUserRelationDTO? CreatedBy { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? LastUpdate { get; set; }

  }
}
