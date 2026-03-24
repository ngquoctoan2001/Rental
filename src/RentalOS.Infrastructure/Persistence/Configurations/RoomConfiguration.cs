using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class RoomConfiguration : IEntityTypeConfiguration<Room>
{
    public void Configure(EntityTypeBuilder<Room> b)
    {
        b.ToTable("rooms");
        b.HasKey(r => r.Id);
        b.HasIndex(r => new { r.PropertyId, r.RoomNumber }).IsUnique();
        b.Property(r => r.RoomNumber).HasMaxLength(20).IsRequired();
        b.Property(r => r.Floor).HasDefaultValue(1);
        b.Property(r => r.AreaSqm).HasColumnType("decimal(6,2)");
        b.Property(r => r.BasePrice).HasColumnType("decimal(12,2)").IsRequired();
        b.Property(r => r.ElectricityPrice).HasColumnType("decimal(8,2)").HasDefaultValue(3500m);
        b.Property(r => r.WaterPrice).HasColumnType("decimal(8,2)").HasDefaultValue(15000m);
        b.Property(r => r.ServiceFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        b.Property(r => r.InternetFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        b.Property(r => r.GarbageFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        b.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        b.Property(r => r.Amenities).HasColumnType("jsonb").HasDefaultValueSql("'[]'");
        b.Property(r => r.Images).HasColumnType("jsonb").HasDefaultValueSql("'[]'");
        b.Property(r => r.MaintenanceSince).HasColumnType("date");

        b.HasOne(r => r.Property).WithMany(p => p.Rooms).HasForeignKey(r => r.PropertyId);
        b.HasIndex(r => r.PropertyId);
        b.HasIndex(r => r.Status);
    }
}
