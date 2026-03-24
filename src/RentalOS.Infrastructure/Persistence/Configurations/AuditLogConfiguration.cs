using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {
        b.ToTable("audit_logs");
        b.HasKey(a => a.Id);
        b.Property(a => a.UserName).HasMaxLength(200);
        b.Property(a => a.Action).HasMaxLength(100).IsRequired();
        b.Property(a => a.EntityType).HasMaxLength(50);
        b.Property(a => a.EntityCode).HasMaxLength(100);
        b.Property(a => a.OldValue).HasColumnType("jsonb");
        b.Property(a => a.NewValue).HasColumnType("jsonb");
        b.Property(a => a.IpAddress).HasMaxLength(45);  // IPv6 max length
        b.Property(a => a.CreatedAt).HasDefaultValueSql("NOW()");

        b.HasIndex(a => new { a.UserId, a.CreatedAt });
        b.HasIndex(a => new { a.EntityType, a.EntityId });
        b.HasIndex(a => a.CreatedAt);
    }
}
