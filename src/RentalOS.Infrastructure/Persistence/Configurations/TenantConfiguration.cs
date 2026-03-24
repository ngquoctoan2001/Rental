using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> b)
    {
        b.ToTable("tenants", "public");
        b.HasKey(t => t.Id);
        b.HasIndex(t => t.Slug).IsUnique();
        b.HasIndex(t => t.OwnerEmail).IsUnique();
        b.Property(t => t.Slug).HasMaxLength(50).IsRequired();
        b.Property(t => t.Name).HasMaxLength(200).IsRequired();
        b.Property(t => t.OwnerEmail).HasMaxLength(255).IsRequired();
        b.Property(t => t.OwnerName).HasMaxLength(200).IsRequired();
        b.Property(t => t.Phone).HasMaxLength(20);
        b.Property(t => t.Plan).HasConversion<string>().HasMaxLength(20).HasDefaultValue("trial");
        b.Property(t => t.SchemaName).HasMaxLength(60).IsRequired();
        b.Property(t => t.IsActive).HasDefaultValue(true);
        b.Property(t => t.OnboardingDone).HasDefaultValue(false);
    }
}

public class SubscriptionPaymentConfiguration : IEntityTypeConfiguration<SubscriptionPayment>
{
    public void Configure(EntityTypeBuilder<SubscriptionPayment> b)
    {
        b.ToTable("subscription_payments", "public");
        b.HasKey(s => s.Id);
        b.Property(s => s.Plan).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(s => s.Amount).HasColumnType("decimal(12,2)").IsRequired();
        b.Property(s => s.Method).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(s => s.ProviderRef).HasMaxLength(200);
        b.Property(s => s.Status).HasMaxLength(20).HasDefaultValue("pending");
        b.Property(s => s.BillingFrom).HasColumnType("date").IsRequired();
        b.Property(s => s.BillingTo).HasColumnType("date").IsRequired();
        b.Property(s => s.CreatedAt).HasDefaultValueSql("NOW()");

        b.HasOne(s => s.Tenant).WithMany(t => t.SubscriptionPayments).HasForeignKey(s => s.TenantId);
        b.HasIndex(s => s.TenantId);
    }
}
