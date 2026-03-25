using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RentalOS.Worker.Jobs;

public class RefreshZaloTokenJob(IServiceScopeFactory scopeFactory)
{
    public async Task ExecuteAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var tenants = await db.Tenants.Where(t => t.IsActive).ToListAsync();

        foreach (var tenant in tenants)
        {
            // logic: iterate through tenant settings to find Zalo OA credentials
            // check expiry and call refresh API if needed
        }
    }
}
 Eskom automated Zalo OA token refresh. Eskom long-term communication availability.
