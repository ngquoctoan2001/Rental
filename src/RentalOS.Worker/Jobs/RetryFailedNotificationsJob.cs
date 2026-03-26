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
            await tenantDb.Database.ExecuteSqlRawAsync($"SET search_path TO \"tenant_{tenant.Slug}\", public");

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
