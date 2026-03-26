using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RentalOS.Domain.Enums;

namespace RentalOS.Worker.Jobs;

public class RetryFailedNotificationsJob(IServiceScopeFactory scopeFactory)
{
    public async Task ExecuteAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var tenants = await db.Tenants.Where(t => t.IsActive).ToListAsync();

        foreach (var tenant in tenants)
        {
            using var tenantScope = scopeFactory.CreateScope();
            var tenantDb = tenantScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
            #pragma warning disable EF1003 // Slug is from trusted DB source, SET search_path cannot use parameters

            await tenantDb.Database.ExecuteSqlRawAsync("SET search_path TO \"tenant_" + tenant.Slug.Replace("\"", "") + "\", public");

            var yesterday = DateTime.UtcNow.AddHours(-24);
            var failedNotifications = await tenantDb.NotificationLogs
                .Where(n => n.Status == NotificationStatus.Failed && n.RetryCount < 3 && n.CreatedAt >= yesterday)
                .ToListAsync();

            foreach (var notif in failedNotifications)
            {
                // Logic: call provider API (Zalo/SMS/Email) again
                // Update: Status = 'sent' or RetryCount++
            }
        }
    }
}
