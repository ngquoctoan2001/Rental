using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InvoiceCode)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(x => x.PartialPaidAmount)
            .HasPrecision(18, 2);

        builder.HasIndex(x => x.InvoiceCode)
            .IsUnique();

        builder.HasIndex(x => x.PaymentLinkToken)
            .IsUnique();

        builder.HasIndex(x => new { x.ContractId, x.BillingMonth })
            .IsUnique()
            .HasFilter("\"Status\" != 'Cancelled'");

        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.Invoice)
            .HasForeignKey(x => x.InvoiceId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
