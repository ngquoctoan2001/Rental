using Microsoft.AspNetCore.Identity;

namespace RentalOS.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>, IEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    
    // Original User fields
    public string? Phone { get; set; }
    public Guid? TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    
    // Status and Security
    public bool IsActive { get; set; } = true;
    public string? InviteToken { get; set; }
    public DateTime? InviteExpiresAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Audit metadata
    public DateTime CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    
    // Navigation
    public virtual ICollection<Contract> ManagedContracts { get; set; } = new List<Contract>();
    public virtual ICollection<MaintenanceTask> AssignedTasks { get; set; } = new List<MaintenanceTask>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
