using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> b)
    {
        b.ToTable("properties");
        b.HasKey(p => p.Id);
        b.Property(p => p.Name).HasMaxLength(200).IsRequired();
        b.Property(p => p.Address).IsRequired();
        b.Property(p => p.Province).HasMaxLength(100);
        b.Property(p => p.District).HasMaxLength(100);
        b.Property(p => p.Ward).HasMaxLength(100);
        b.Property(p => p.Lat).HasColumnType("decimal(10,7)");
        b.Property(p => p.Lng).HasColumnType("decimal(10,7)");
        b.Property(p => p.CoverImage).HasMaxLength(500);
        b.Property(p => p.Images).HasColumnType("jsonb").HasDefaultValueSql("'[]'");
        b.Property(p => p.TotalFloors).HasDefaultValue(1);
        b.Property(p => p.IsActive).HasDefaultValue(true);
    }
}
