using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Email)
            .IsUnique();

        builder.HasIndex(x => x.InviteToken);

        // Ignore navs that don't exist in per-tenant DDL to prevent shadow FK columns
        builder.Ignore(x => x.ManagedContracts);
        builder.Ignore(x => x.AssignedTasks);
        builder.Ignore(x => x.Reviews);
        builder.Ignore(x => x.Addresses);
    }
}
