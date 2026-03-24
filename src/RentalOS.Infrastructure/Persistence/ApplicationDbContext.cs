using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;


namespace RentalOS.Infrastructure.Persistence;

/// <summary>Main EF Core context. The active PostgreSQL search_path is set per request by TenantMiddleware.</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : IdentityDbContext<User, ApplicationRole, Guid>(options), IApplicationDbContext
{

    // ── Per-tenant tables ────────────────────────────────────────────────────
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Contract> Contracts => Set<Contract>();
    public DbSet<ContractCoTenant> ContractCoTenants => Set<ContractCoTenant>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MeterReading> MeterReadings => Set<MeterReading>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<AiConversation> AiConversations => Set<AiConversation>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<PaymentLinkLog> PaymentLinkLogs => Set<PaymentLinkLog>();

    // ── Public-schema tables ─────────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<SubscriptionPayment> SubscriptionPayments => Set<SubscriptionPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    /// <summary>Automatically stamps UpdatedAt on any entity that inherits BaseEntity before saving.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        // Also stamp Setting.UpdatedAt (not inheriting BaseEntity)
        foreach (var entry in ChangeTracker.Entries<Setting>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }
}
