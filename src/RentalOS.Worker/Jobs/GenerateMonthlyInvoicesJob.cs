using RentalOS.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RentalOS.Worker.Jobs;

public class GenerateMonthlyInvoicesJob(
    IServiceScopeFactory scopeFactory,
    ILogger<GenerateMonthlyInvoicesJob> logger)
{
    public async Task ExecuteAsync()
    {
        using var scope = scopeFactory.CreateScope();
        var dbContent = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        
        // 1. Lấy danh sách tenant active
        var tenants = await dbContent.Tenants.Where(t => t.IsActive).ToListAsync();

        foreach (var tenant in tenants)
        {
            try
            {
                using var tenantScope = scopeFactory.CreateScope();
                var tenantDb = tenantScope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
                
                // Switch schema logic (via middleware-like or direct command)
                #pragma warning disable EF1003 // Slug is from trusted DB source, SET search_path cannot use parameters

                await tenantDb.Database.ExecuteSqlRawAsync("SET search_path TO \"tenant_" + tenant.Slug.Replace("\"", "") + "\", public");

                // logic tạo hóa đơn: iterate contracts -> check meter readings -> create invoice
                logger.LogInformation("Đang xử lý tenant {Slug}...", tenant.Slug);
                
                // Placeholder cho logic nghiệp vụ chi tiết
                // ...
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Lỗi khi tạo hóa đơn cho tenant {Slug}", tenant.Slug);
            }
        }
    }
}
