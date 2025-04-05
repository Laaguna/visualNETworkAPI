public class UserPublicDto
{
    public int Id { get; set; }
    public string User { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateOnly? DateBirth { get; set; }
    public bool Active { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? Genre { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? LastUpdate { get; set; }

  // Excluye la propiedad 'Password'
}