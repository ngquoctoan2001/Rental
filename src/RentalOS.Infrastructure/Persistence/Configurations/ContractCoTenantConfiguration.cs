using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class ContractCoTenantConfiguration : IEntityTypeConfiguration<ContractCoTenant>
{
    public void Configure(EntityTypeBuilder<ContractCoTenant> builder)
    {
        builder.ToTable("contract_co_tenants");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.ContractId, x.CustomerId })
            .IsUnique();
    }
}
