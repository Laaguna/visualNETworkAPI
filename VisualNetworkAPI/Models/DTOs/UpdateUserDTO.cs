public class UserUpdateDto
{
    
    public string? FirstName { get; set; } // Propiedades opcionales para la actualización
    
    public string? LastName { get; set; }
    
    public string? Email { get; set; }
    
    public string? Phone { get; set; }
  
    public string? Address { get; set; }
    
    public string? Genre { get; set; }
    // No incluir Password aquí
}