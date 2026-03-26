using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using RentalOS.Application.Common.Interfaces;
using System.Data;

namespace RentalOS.Infrastructure.Persistence;

/// <summary>Main EF Core context. The active PostgreSQL search_path is set per request by TenantMiddleware.</summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) 
    : Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<RentalOS.Domain.Entities.User, RentalOS.Domain.Entities.ApplicationRole, Guid>(options), IApplicationDbContext
{

    // ── Per-tenant tables ────────────────────────────────────────────────────
    public DbSet<Domain.Entities.Property> Properties => Set<Domain.Entities.Property>();
    public DbSet<Domain.Entities.Room> Rooms => Set<Domain.Entities.Room>();
    public DbSet<Domain.Entities.Customer> Customers => Set<Domain.Entities.Customer>();
    public DbSet<Domain.Entities.Contract> Contracts => Set<Domain.Entities.Contract>();
    public DbSet<Domain.Entities.ContractCoTenant> ContractCoTenants => Set<Domain.Entities.ContractCoTenant>();
    public DbSet<Domain.Entities.Invoice> Invoices => Set<Domain.Entities.Invoice>();
    public DbSet<Domain.Entities.Transaction> Transactions => Set<Domain.Entities.Transaction>();
    public DbSet<Domain.Entities.MeterReading> MeterReadings => Set<Domain.Entities.MeterReading>();
    public DbSet<Domain.Entities.NotificationLog> NotificationLogs => Set<Domain.Entities.NotificationLog>();
    public DbSet<Domain.Entities.AiConversation> AiConversations => Set<Domain.Entities.AiConversation>();
    public DbSet<Domain.Entities.Setting> Settings => Set<Domain.Entities.Setting>();
    public DbSet<Domain.Entities.AuditLog> AuditLogs => Set<Domain.Entities.AuditLog>();
    public DbSet<Domain.Entities.PaymentLinkLog> PaymentLinkLogs => Set<Domain.Entities.PaymentLinkLog>();

    // ── Public-schema tables ─────────────────────────────────────────────────
    public DbSet<Domain.Entities.Tenant> Tenants => Set<Domain.Entities.Tenant>();
    public DbSet<Domain.Entities.User> ApplicationUsers => Set<Domain.Entities.User>();
    public new DbSet<Domain.Entities.ApplicationRole> Roles => Set<Domain.Entities.ApplicationRole>();
    public DbSet<Domain.Entities.SubscriptionPayment> SubscriptionPayments => Set<Domain.Entities.SubscriptionPayment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Tenant-schema tables (created by TenantSchemaManager DDL) use snake_case column names
        // and store enums as VARCHAR. Public-schema tables (AspNet* Identity, tenants,
        // subscription_payments) use PascalCase + integer enums — skip those.
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName() ?? string.Empty;
            var schema = entity.GetSchema();

            // Skip: AspNet* Identity tables, the public Users table, and explicit public-schema tables
            if (tableName.StartsWith("AspNet", StringComparison.OrdinalIgnoreCase)
                || tableName == "users"
                || schema == "public")
                continue;

            foreach (var property in entity.GetProperties())
            {
                if (property.GetColumnName() == property.Name)
                    property.SetColumnName(ToSnakeCase(property.Name));

                // Tenant DDL stores enums as VARCHAR → convert enum properties to strings
                var clrType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
                if (clrType.IsEnum && property.GetValueConverter() is null)
                {
                    var converterType = typeof(EnumToStringConverter<>).MakeGenericType(clrType);
                    var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                    property.SetValueConverter(converter);
                    if (property.GetMaxLength() is null)
                        property.SetMaxLength(50);
                }
            }
            foreach (var key in entity.GetKeys())
                key.SetName(ToSnakeCase(key.GetName()!));
            foreach (var fk in entity.GetForeignKeys())
                fk.SetConstraintName(ToSnakeCase(fk.GetConstraintName()!));
            foreach (var index in entity.GetIndexes())
                index.SetDatabaseName(ToSnakeCase(index.GetDatabaseName()!));
        }
    }

    private static string ToSnakeCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0)
                sb.Append('_');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }

    /// <summary>Automatically stamps UpdatedAt on any entity that inherits BaseEntity before saving.</summary>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries<Domain.Entities.BaseEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        // Also stamp Setting.UpdatedAt (not inheriting BaseEntity)
        foreach (var entry in ChangeTracker.Entries<Domain.Entities.Setting>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
        }
        return base.SaveChangesAsync(cancellationToken);
    }

    public async Task<Domain.Entities.Tenant?> FindTenantByInvoiceTokenAsync(string token)
    {
        // 1. Get all schemas that start with "tenant_" from public.tenants
        var schemas = await Tenants.Select(t => t.SchemaName).ToListAsync();
        if (!schemas.Any()) return null;

        // 2. Build a UNION query to find which schema has this token
        // Use lowercase 'payment_link_token' to match TenantSchemaManager DDL
        var queryBuilder = new System.Text.StringBuilder();
        for (int i = 0; i < schemas.Count; i++)
        {
            var s = schemas[i];
            // PostgreSQL requires double quotes for schema names if they have special chars, 
            // and we must match the column name produced by the DDL (snake_case)
            queryBuilder.Append($"SELECT '{s}' as \"SchemaName\" FROM \"{s}\".invoices WHERE \"payment_link_token\" = @p0");
            if (i < schemas.Count - 1) queryBuilder.Append(" UNION ALL ");
        }

        var sql = queryBuilder.ToString();
        
        var connection = Database.GetDbConnection();
        if (connection.State != ConnectionState.Open) await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var rToken = command.CreateParameter();
        rToken.ParameterName = "@p0";
        rToken.Value = token;
        command.Parameters.Add(rToken);

        string? foundSchema = null;
        try 
        {
            using (var reader = await command.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    foundSchema = reader.GetString(0);
                }
            }
        }
        catch (Exception ex)
        {
            // Log if specific schema/table doesn't exist yet
            System.Diagnostics.Debug.WriteLine($"Query failed: {ex.Message}");
            return null;
        }

        if (string.IsNullOrEmpty(foundSchema)) return null;

        return await Tenants.FirstOrDefaultAsync(t => t.SchemaName == foundSchema);
    }
}
