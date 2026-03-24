using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RentalOS.Domain.Entities;
using RentalOS.Infrastructure.Multitenancy;
using System.Text.Json;

namespace RentalOS.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core SaveChanges interceptor that automatically creates AuditLog entries
/// for every Added, Modified, or Deleted entity that carries an Id (Guid).
/// </summary>
public sealed class AuditInterceptor(ITenantContext tenantContext) : SaveChangesInterceptor
{
    private readonly ITenantContext _tenantContext = tenantContext;

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not ApplicationDbContext ctx) return result;
        if (!_tenantContext.IsInitialized) return result;

        var auditEntries = BuildAuditLogs(ctx);

        if (auditEntries.Count > 0)
            await ctx.AuditLogs.AddRangeAsync(auditEntries, cancellationToken);

        return result;
    }

    private List<AuditLog> BuildAuditLogs(ApplicationDbContext ctx)
    {
        var logs = new List<AuditLog>();
        var now = DateTime.UtcNow;

        foreach (var entry in ctx.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog or PaymentLinkLog) continue;  // don't audit audit logs themselves
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted)) continue;

            // Get entity Id
            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp?.CurrentValue is not Guid entityId) continue;

            var entityType = entry.Entity.GetType().Name;
            var action = entry.State switch
            {
                EntityState.Added    => $"{ToSnake(entityType)}.create",
                EntityState.Modified => $"{ToSnake(entityType)}.update",
                EntityState.Deleted  => $"{ToSnake(entityType)}.delete",
                _                    => "unknown"
            };

            string? oldValue = null;
            string? newValue = null;

            if (entry.State == EntityState.Modified)
            {
                var changed = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                var original = entry.Properties
                    .Where(p => p.IsModified)
                    .ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                newValue = JsonSerializer.Serialize(changed);
                oldValue = JsonSerializer.Serialize(original);
            }
            else if (entry.State == EntityState.Added)
            {
                var vals = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.CurrentValue);
                newValue = JsonSerializer.Serialize(vals);
            }
            else if (entry.State == EntityState.Deleted)
            {
                var vals = entry.Properties.ToDictionary(p => p.Metadata.Name, p => p.OriginalValue);
                oldValue = JsonSerializer.Serialize(vals);
            }

            // Extract entity code (ContractCode, InvoiceCode, etc.) if present
            var codeProp = entry.Properties.FirstOrDefault(p =>
                p.Metadata.Name is "ContractCode" or "InvoiceCode" or "TransactionCode" or "RoomNumber");
            string? entityCode = codeProp?.CurrentValue as string;

            logs.Add(new AuditLog
            {
                Id = Guid.NewGuid(),
                UserId = _tenantContext.UserId == Guid.Empty ? null : _tenantContext.UserId,
                UserName = null,  // resolved by caller layer if needed
                Action = action,
                EntityType = entityType,
                EntityId = entityId,
                EntityCode = entityCode,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedAt = now
            });
        }

        return logs;
    }

    private static string ToSnake(string s) =>
        string.Concat(s.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + c : c.ToString()))
              .ToLowerInvariant();
}
