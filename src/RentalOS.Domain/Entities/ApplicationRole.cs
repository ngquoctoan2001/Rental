using Microsoft.AspNetCore.Identity;

namespace RentalOS.Domain.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }
}
