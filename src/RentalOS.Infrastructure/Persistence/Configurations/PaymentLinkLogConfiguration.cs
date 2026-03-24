using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class PaymentLinkLogConfiguration : IEntityTypeConfiguration<PaymentLinkLog>
{
    public void Configure(EntityTypeBuilder<PaymentLinkLog> builder)
    {
        builder.ToTable("payment_link_logs");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.InvoiceId);
    }
}
