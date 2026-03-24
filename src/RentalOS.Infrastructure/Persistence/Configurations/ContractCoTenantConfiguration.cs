using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class ContractCoTenantConfiguration : IEntityTypeConfiguration<ContractCoTenant>
{
    public void Configure(EntityTypeBuilder<ContractCoTenant> b)
    {
        b.ToTable("contract_co_tenants");
        b.HasKey(c => c.Id);
        b.HasIndex(c => new { c.ContractId, c.CustomerId }).IsUnique();
        b.Property(c => c.IsPrimary).HasDefaultValue(false);
        b.Property(c => c.MovedInAt).HasColumnType("date");
        b.Property(c => c.MovedOutAt).HasColumnType("date");

        b.HasOne(c => c.Contract).WithMany(ct => ct.CoTenants)
            .HasForeignKey(c => c.ContractId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(c => c.Customer).WithMany().HasForeignKey(c => c.CustomerId);

        b.HasIndex(c => c.ContractId);
        b.HasIndex(c => c.CustomerId);
    }
}
