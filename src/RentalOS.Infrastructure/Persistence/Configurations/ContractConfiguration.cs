using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contracts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContractCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.MonthlyRent)
            .HasPrecision(18, 2);

        builder.Property(x => x.DepositAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.ContractCode)
            .IsUnique();

        builder.HasIndex(x => x.EndDate)
            .HasFilter("\"Status\" = 0");

        builder.HasMany(x => x.CoTenants)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(x => x.Invoices)
            .WithOne(x => x.Contract)
            .HasForeignKey(x => x.ContractId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
