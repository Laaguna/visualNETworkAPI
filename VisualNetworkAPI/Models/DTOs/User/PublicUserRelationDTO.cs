namespace VisualNetworkAPI.Models.DTOs
{
  public class PublicUserRelationDTO
  {
    public int Id { get; set; }
    public string User1 { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
  }
}
