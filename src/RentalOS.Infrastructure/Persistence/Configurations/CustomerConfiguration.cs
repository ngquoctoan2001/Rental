using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> b)
    {
        b.ToTable("customers");
        b.HasKey(c => c.Id);
        b.Property(c => c.FullName).HasMaxLength(200).IsRequired();
        b.Property(c => c.Phone).HasMaxLength(20).IsRequired();
        b.Property(c => c.Email).HasMaxLength(255);
        b.Property(c => c.IdCardNumber).HasMaxLength(20);
        b.Property(c => c.IdCardImageFront).HasMaxLength(500);
        b.Property(c => c.IdCardImageBack).HasMaxLength(500);
        b.Property(c => c.PortraitImage).HasMaxLength(500);
        b.Property(c => c.DateOfBirth).HasColumnType("date");
        b.Property(c => c.Gender).HasMaxLength(10);
        b.Property(c => c.Occupation).HasMaxLength(200);
        b.Property(c => c.Workplace).HasMaxLength(200);
        b.Property(c => c.EmergencyContactName).HasMaxLength(200);
        b.Property(c => c.EmergencyContactPhone).HasMaxLength(20);
        b.Property(c => c.EmergencyContactRelationship).HasMaxLength(50);
        b.Property(c => c.IsBlacklisted).HasDefaultValue(false);

        b.HasIndex(c => c.Phone);
        b.HasIndex(c => c.IdCardNumber)
            .HasFilter("id_card_number IS NOT NULL");
    }
}
