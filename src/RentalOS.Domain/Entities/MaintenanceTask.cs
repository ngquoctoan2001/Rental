namespace RentalOS.Domain.Entities;

public enum MaintenancePriority { Low, Medium, High, Critical }
public enum MaintenanceStatus { Pending, InProgress, Completed, Cancelled }

public class MaintenanceTask : BaseEntity, IEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaintenancePriority Priority { get; set; }
    public MaintenanceStatus Status { get; set; }
    
    public Guid PropertyId { get; set; }
    public Property? Property { get; set; }
    
    public Guid? AssignedToId { get; set; }
    public ApplicationUser? AssignedTo { get; set; }
}
