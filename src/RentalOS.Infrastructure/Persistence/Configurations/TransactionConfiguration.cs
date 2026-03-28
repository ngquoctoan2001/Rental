using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasPrecision(18, 2);

        builder.Property(x => x.ProviderRef)
            .HasMaxLength(200);

        builder.Property(x => x.ProviderResponse)
            .HasColumnType("jsonb");

        builder.HasIndex(x => x.ProviderRef);
        builder.HasIndex(x => x.PaidAt);
    }
}
