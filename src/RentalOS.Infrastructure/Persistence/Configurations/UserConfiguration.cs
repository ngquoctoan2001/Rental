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

        // ManagedContracts is not mapped via any FK config, so ignore to prevent shadow column
        builder.Ignore(x => x.ManagedContracts);
        // Note: AssignedTasks, Reviews, Addresses are mapped via their own IEntityTypeConfiguration
        // (MaintenanceTaskConfiguration, ReviewConfiguration, AddressConfiguration) using WithMany(),
        // so they must NOT be ignored here to avoid EF Core warnings.
    }
}
