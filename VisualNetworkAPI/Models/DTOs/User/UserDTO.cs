namespace VisualNetworkAPI.Models.DTOs
{
  public class UserDTO
  {
    public required string User { get; set; }
    public required string Password { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required bool Active { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Genre { get; set; }
    public string? Avatar { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }
    

  }
}
