using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class ReconcilePaymentsJob(IServiceScopeFactory scopeFactory)
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

            var yesterday = DateTime.UtcNow.AddDays(-1);
            var pendingTransactions = await tenantDb.Transactions
                .Where(t => t.Status == TransactionStatus.Pending && t.CreatedAt >= yesterday)
                .ToListAsync();

            foreach (var tx in pendingTransactions)
            {
                // Call MoMo/VNPay Query Status API
                // If Paid -> Update Transaction & Invoice
                // If Failed/Expired -> Update Transaction
            }
        }
    }
}
