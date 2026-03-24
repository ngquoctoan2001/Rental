using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class PaymentLinkLogConfiguration : IEntityTypeConfiguration<PaymentLinkLog>
{
    public void Configure(EntityTypeBuilder<PaymentLinkLog> b)
    {
        b.ToTable("payment_link_logs");
        b.HasKey(p => p.Id);
        b.Property(p => p.IpAddress).HasMaxLength(45);
        b.Property(p => p.Action).HasMaxLength(30).IsRequired();
        b.Property(p => p.CreatedAt).HasDefaultValueSql("NOW()");

        b.HasOne(p => p.Invoice).WithMany().HasForeignKey(p => p.InvoiceId);
        b.HasIndex(p => p.InvoiceId);
    }
}
