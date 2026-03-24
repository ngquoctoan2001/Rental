namespace RentalOS.Domain.Entities;

public class Address : BaseEntity
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = "Vietnam";
    
    // Ownership
    public Guid? UserId { get; set; }
    public User? User { get; set; }
    
    public Guid? PropertyId { get; set; }
    public Property? Property { get; set; }
}
