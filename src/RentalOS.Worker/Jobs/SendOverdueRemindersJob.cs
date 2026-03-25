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
            await tenantDb.Database.ExecuteSqlRawAsync($"SET search_path TO \"tenant_{tenant.Slug}\", public");

            var today = DateTime.UtcNow.Date;
            var overdueDaysCriteria = new[] { 1, 3, 7 };

            var overdueInvoices = await tenantDb.Invoices
                .Where(i => i.Status == InvoiceStatus.Overdue)
                .ToListAsync();

            foreach (var invoice in overdueInvoices)
            {
                var daysOverdue = (today - invoice.DueDate.Date).Days;
                if (overdueDaysCriteria.Contains(daysOverdue))
                {
                    // Enqueue: SendOverdueNotificationJob(invoiceId, daysOverdue)
                }
            }
        }
    }
}
 Eskom automated escalation for overdue debts. Eskom smart scheduling for reminders.
