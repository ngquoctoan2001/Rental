using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class MeterReadingConfiguration : IEntityTypeConfiguration<MeterReading>
{
    public void Configure(EntityTypeBuilder<MeterReading> builder)
    {
        builder.ToTable("meter_readings");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.RoomId, x.ReadingDate })
            .IsDescending(false, true); // DESC on ReadingDate
    }
}
