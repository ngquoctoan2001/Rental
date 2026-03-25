using Microsoft.AspNetCore.Identity;

namespace RentalOS.Domain.Entities;

public class User : IdentityUser<Guid>, IEntity
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Phone { get; set; }
    public string Role { get; set; } = string.Empty; // owner | manager | staff
    public Guid? TenantId { get; set; }
    
    // Status
    public bool IsActive { get; set; } = true;
    public string? InviteToken { get; set; }
    public DateTime? InviteExpiresAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    /// <summary>Danh sách UUID nhà trọ nhân viên được phép quản lý (Mapping JSONB).</summary>
    public List<Guid> AssignedPropertyIds { get; set; } = [];
    
    // Audit metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public string? UpdatedBy { get; set; }
    
    // Navigation (Optional based on need, but keep for now)
    public virtual ICollection<Contract> ManagedContracts { get; set; } = new List<Contract>();
    public virtual ICollection<MaintenanceTask> AssignedTasks { get; set; } = new List<MaintenanceTask>();
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();
}
