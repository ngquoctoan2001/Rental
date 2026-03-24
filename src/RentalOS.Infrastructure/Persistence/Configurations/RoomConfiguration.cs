using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> builder)
    {
        builder.ToTable("rooms");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.BasePrice)
            .HasPrecision(18, 2);

        builder.Property(x => x.AreaSqm)
            .HasPrecision(10, 2);

        builder.HasIndex(x => new { x.PropertyId, x.RoomNumber })
            .IsUnique();
            
        builder.HasMany(x => x.Contracts)
            .WithOne(x => x.Room)
            .HasForeignKey(x => x.RoomId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
