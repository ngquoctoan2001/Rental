using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
{
    public void Configure(EntityTypeBuilder<NotificationLog> b)
    {
        b.ToTable("notification_logs");
        b.HasKey(n => n.Id);
        b.Property(n => n.EventType).HasMaxLength(50).IsRequired();
        b.Property(n => n.Channel).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(n => n.RecipientPhone).HasMaxLength(20);
        b.Property(n => n.RecipientEmail).HasMaxLength(255);
        b.Property(n => n.RecipientName).HasMaxLength(200);
        b.Property(n => n.Subject).HasMaxLength(300);
        b.Property(n => n.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue("pending");
        b.Property(n => n.ProviderRef).HasMaxLength(200);
        b.Property(n => n.RetryCount).HasDefaultValue(0);
        b.Property(n => n.EntityType).HasMaxLength(50);

        b.HasIndex(n => new { n.Status, n.CreatedAt });
        b.HasIndex(n => new { n.EntityType, n.EntityId });
        b.Ignore(n => n.UpdatedAt);  // not in schema, no updated_at column
    }
}
