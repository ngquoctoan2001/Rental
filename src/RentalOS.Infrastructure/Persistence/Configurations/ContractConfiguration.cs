using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> b)
    {
        b.ToTable("contracts");
        b.HasKey(c => c.Id);
        b.HasIndex(c => c.ContractCode).IsUnique();
        b.Property(c => c.ContractCode).HasMaxLength(50).IsRequired();
        b.Property(c => c.StartDate).HasColumnType("date").IsRequired();
        b.Property(c => c.EndDate).HasColumnType("date").IsRequired();
        b.Property(c => c.MonthlyRent).HasColumnType("decimal(12,2)").IsRequired();
        b.Property(c => c.DepositMonths).HasDefaultValue(1);
        b.Property(c => c.DepositAmount).HasColumnType("decimal(12,2)").IsRequired();
        b.Property(c => c.DepositPaid).HasDefaultValue(false);
        b.Property(c => c.DepositRefunded).HasColumnType("decimal(12,2)");
        b.Property(c => c.ElectricityPrice).HasColumnType("decimal(8,2)");
        b.Property(c => c.WaterPrice).HasColumnType("decimal(8,2)");
        b.Property(c => c.ServiceFee).HasColumnType("decimal(10,2)");
        b.Property(c => c.InternetFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        b.Property(c => c.GarbageFee).HasColumnType("decimal(10,2)").HasDefaultValue(0m);
        b.Property(c => c.BillingDate).HasDefaultValue(5);
        b.Property(c => c.PaymentDueDays).HasDefaultValue(10);
        b.Property(c => c.MaxOccupants).HasDefaultValue(2);
        b.Property(c => c.TemplateId).HasMaxLength(50);
        b.Property(c => c.PdfUrl).HasMaxLength(500);
        b.Property(c => c.Status).HasConversion<string>().HasMaxLength(20).HasDefaultValue("active");
        b.Property(c => c.TerminationType).HasConversion<string>().HasMaxLength(30);
        b.Property(c => c.SignedByCustomer).HasDefaultValue(false);

        b.HasOne(c => c.Room).WithMany(r => r.Contracts).HasForeignKey(c => c.RoomId);
        b.HasOne(c => c.Customer).WithMany(cu => cu.Contracts).HasForeignKey(c => c.CustomerId);

        b.HasIndex(c => c.RoomId);
        b.HasIndex(c => c.CustomerId);
        b.HasIndex(c => c.Status);
        b.HasIndex(c => c.EndDate).HasFilter("status = 'active'");
    }
}
