using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.IpAddress)
            .HasColumnType("text");

        builder.Property(x => x.OldValue)
            .HasColumnType("jsonb");

        builder.Property(x => x.NewValue)
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.CreatedAt)
            .IsDescending();
    }
}
