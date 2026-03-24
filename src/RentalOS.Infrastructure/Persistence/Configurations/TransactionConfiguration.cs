using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> b)
    {
        b.ToTable("transactions");
        b.HasKey(t => t.Id);
        b.Property(t => t.TransactionCode).HasMaxLength(100);
        b.Property(t => t.Amount).HasColumnType("decimal(12,2)").IsRequired();
        b.Property(t => t.Method).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(t => t.Direction).HasConversion<string>().HasMaxLength(10).IsRequired();
        b.Property(t => t.Category).HasConversion<string>().HasMaxLength(30).HasDefaultValue("rent");
        b.Property(t => t.ProviderRef).HasMaxLength(200);
        b.Property(t => t.ProviderResponse).HasColumnType("jsonb");
        b.Property(t => t.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue("success");
        b.Property(t => t.Note).HasMaxLength(500);
        b.Property(t => t.ReceiptUrl).HasMaxLength(500);
        b.Property(t => t.PaidAt).HasDefaultValueSql("NOW()");

        b.HasOne(t => t.Invoice).WithMany(i => i.Transactions).HasForeignKey(t => t.InvoiceId);

        b.HasIndex(t => t.InvoiceId);
        b.HasIndex(t => t.ProviderRef).HasFilter("provider_ref IS NOT NULL");
        b.HasIndex(t => t.PaidAt);
        b.HasIndex(t => t.Method);
    }
}
