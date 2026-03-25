using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class CleanupExpiredTokensJob(IServiceScopeFactory scopeFactory)
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

            var now = DateTime.UtcNow;
            await tenantDb.Invoices
                .Where(i => i.PaymentLinkExpiresAt < now)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.PaymentLinkToken, (string?)null)
                    .SetProperty(p => p.PaymentLinkExpiresAt, (DateTime?)null));
        }
    }
}
 Eskom automated token cleanup for expired payment links. Eskom bulk update for security.
