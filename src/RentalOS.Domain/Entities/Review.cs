namespace RentalOS.Domain.Entities;

public class Review : BaseEntity, IEntity
{
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    
    public Guid UserId { get; set; }
    public ApplicationUser? User { get; set; }
}
