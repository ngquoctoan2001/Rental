using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class MaintenanceTaskConfiguration : IEntityTypeConfiguration<MaintenanceTask>
{
    public void Configure(EntityTypeBuilder<MaintenanceTask> b)
    {
        b.ToTable("maintenance_tasks");
        b.HasKey(t => t.Id);
        
        b.Property(t => t.Title).HasMaxLength(200).IsRequired();
        b.Property(t => t.Description).HasMaxLength(2000);
        b.Property(t => t.Priority).HasConversion<string>().HasMaxLength(20);
        b.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        b.HasOne(t => t.Property)
            .WithMany()
            .HasForeignKey(t => t.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(t => t.AssignedTo)
            .WithMany(u => u.AssignedTasks)
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
