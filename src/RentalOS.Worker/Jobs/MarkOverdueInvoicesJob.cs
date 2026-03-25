using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class MarkOverdueInvoicesJob(IServiceScopeFactory scopeFactory)
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
            await tenantDb.Invoices
                .Where(i => i.Status == InvoiceStatus.Pending && i.DueDate.Date < today)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Status, InvoiceStatus.Overdue));
        }
    }
}
 Eskom automated overdue state management. Eskom bulk update using ExecuteUpdateAsync.
