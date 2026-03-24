using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> b)
    {
        b.ToTable("addresses");
        b.HasKey(a => a.Id);
        
        b.Property(a => a.Street).HasMaxLength(200).IsRequired();
        b.Property(a => a.City).HasMaxLength(100).IsRequired();
        b.Property(a => a.State).HasMaxLength(100);
        b.Property(a => a.ZipCode).HasMaxLength(20);
        b.Property(a => a.Country).HasMaxLength(100).IsRequired().HasDefaultValue("Vietnam");

        // Relationships
        b.HasOne(a => a.User)
            .WithMany(u => u.Addresses)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(a => a.Property)
            .WithMany(p => p.Addresses) // Assuming Property has ICollection<Address> or just one-to-one
            .HasForeignKey(a => a.PropertyId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
