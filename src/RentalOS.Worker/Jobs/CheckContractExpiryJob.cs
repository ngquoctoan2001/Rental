using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class CheckContractExpiryJob(IServiceScopeFactory scopeFactory)
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

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var alertDays = new[] { 30, 15, 7 };

            var contracts = await tenantDb.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            foreach (var contract in contracts)
            {
                var daysRemaining = (contract.EndDate.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
                if (alertDays.Contains(daysRemaining))
                {
                    // Enqueue: SendContractExpiryAlertJob(contract.Id)
                }
            }
        }
    }
}
