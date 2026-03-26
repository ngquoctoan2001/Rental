using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class SendInvoiceRemindersJob(IServiceScopeFactory scopeFactory)
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var remindDays = new[] { 3, 1 }; // From settings placeholder

            var invoices = await tenantDb.Invoices
                .Where(i => i.Status == InvoiceStatus.Pending)
                .Where(i => remindDays.Contains((i.DueDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days))
                .ToListAsync();

            foreach (var invoice in invoices)
            {
                // Enqueue notification (e.g. Zalo)
                // logic: check notification_logs to avoid double sending
            }
        }
    }
}
