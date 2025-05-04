public class UserPublicDto
{
    public int Id { get; set; }
    required public string  User { get; set; }
    required public string Email { get; set; }
    required public string FirstName { get; set; }
    required public string LastName { get; set; }
    public DateOnly? DateBirth { get; set; }
    public bool Active { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Genre { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }

}
