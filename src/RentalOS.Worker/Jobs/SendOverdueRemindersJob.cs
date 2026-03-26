using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class SendOverdueRemindersJob(IServiceScopeFactory scopeFactory)
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
            var overdueDaysCriteria = new[] { 1, 3, 7 };

            var overdueInvoices = await tenantDb.Invoices
                .Where(i => i.Status == InvoiceStatus.Overdue)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                var daysOverdue = (today.ToDateTime(TimeOnly.MinValue) - invoice.DueDate.ToDateTime(TimeOnly.MinValue)).Days;
                if (overdueDaysCriteria.Contains(daysOverdue))
                {
                    // Enqueue: SendOverdueNotificationJob(invoiceId, daysOverdue)
                }
            }
        }
    }
}
