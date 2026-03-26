using MediatR;
using Microsoft.Extensions.Caching.Memory;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Settings.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RentalOS.Application.Modules.Settings.Queries.GetAllSettings;

public record GetAllSettingsQuery : IRequest<AllSettingsDto>;

public class GetAllSettingsQueryHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext,
    IMemoryCache cache) : IRequestHandler<GetAllSettingsQuery, AllSettingsDto>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<AllSettingsDto> Handle(GetAllSettingsQuery request, CancellationToken cancellationToken)
    {
        // 1. Get from cache
        string cacheKey = $"settings:{tenantContext.TenantId}";
        if (cache.TryGetValue(cacheKey, out AllSettingsDto? cachedSettings) && cachedSettings != null)
        {
            return cachedSettings;
        }

        // 2. Query from DB
        var settings = await dbContext.Settings
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new AllSettingsDto();

        foreach (var setting in settings)
        {
            switch (setting.Key.ToLower())
            {
                case "payment.momo":
                    result.MoMo = JsonSerializer.Deserialize<MoMoSettingsDto>(setting.Value, JsonOptions) ?? new();
                    break;
                case "payment.vnpay":
                    result.VNPay = JsonSerializer.Deserialize<VNPaySettingsDto>(setting.Value, JsonOptions) ?? new();
                    break;
                case "payment.bank":
                    result.Bank = JsonSerializer.Deserialize<BankSettingsDto>(setting.Value, JsonOptions) ?? new();
                    break;
                case "company":
                    result.Company = JsonSerializer.Deserialize<CompanySettingsDto>(setting.Value, JsonOptions) ?? new();
                    break;
                case "billing":
                    result.Billing = JsonSerializer.Deserialize<BillingSettingsDto>(setting.Value, JsonOptions) ?? new();
                    break;
            }
        }

        // 3. Set to cache
        cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

        return result;
    }
}
