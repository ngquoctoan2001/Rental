using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> b)
    {
        b.ToTable("meter_readings");
        b.HasKey(m => m.Id);
        b.Property(m => m.ReadingDate).HasColumnType("date").IsRequired();
        b.Property(m => m.ElectricityImage).HasMaxLength(500);
        b.Property(m => m.WaterImage).HasMaxLength(500);
        b.Property(m => m.Note).HasMaxLength(200);

        b.HasOne(m => m.Room).WithMany().HasForeignKey(m => m.RoomId);

        b.HasIndex(m => m.RoomId);
        b.HasIndex(m => new { m.RoomId, m.ReadingDate });  // for DESC lookup
    }
}
