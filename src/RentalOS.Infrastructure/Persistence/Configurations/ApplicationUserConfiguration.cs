using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> b)
    {
        b.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        b.Property(u => u.Phone).HasMaxLength(20);
        b.Property(u => u.AvatarUrl).HasMaxLength(500);
        b.Property(u => u.IsActive).HasDefaultValue(true);
        b.Property(u => u.InviteToken).HasMaxLength(100);
        
        b.HasIndex(u => u.InviteToken)
            .HasFilter("invite_token IS NOT NULL");

        // Multi-tenancy
        b.HasOne(u => u.Tenant)
            .WithMany()
            .HasForeignKey(u => u.TenantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
