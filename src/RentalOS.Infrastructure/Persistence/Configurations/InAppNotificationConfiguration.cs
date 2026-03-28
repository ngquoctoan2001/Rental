using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class InAppNotificationConfiguration : IEntityTypeConfiguration<InAppNotification>
{
    public void Configure(EntityTypeBuilder<InAppNotification> builder)
    {
        builder.ToTable("in_app_notifications");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.Type)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(x => x.Message)
            .IsRequired();

        builder.Property(x => x.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => new { x.UserId, x.IsRead });
        builder.HasIndex(x => x.CreatedAt).IsDescending();
    }
}
