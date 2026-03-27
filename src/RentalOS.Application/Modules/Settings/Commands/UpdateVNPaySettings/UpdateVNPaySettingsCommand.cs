using MediatR;
using Microsoft.Extensions.Caching.Memory;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Settings.Dtos;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace RentalOS.Application.Modules.Settings.Commands.UpdateVNPaySettings;

public record UpdateVNPaySettingsCommand(VNPaySettingsDto Settings) : IRequest<bool>;

public class UpdateVNPaySettingsCommandHandler(
    IApplicationDbContext dbContext,
    ITenantContext tenantContext,
    IMemoryCache cache) : IRequestHandler<UpdateVNPaySettingsCommand, bool>
{
    private const string SettingKey = "payment.vnpay";

    public async Task<bool> Handle(UpdateVNPaySettingsCommand request, CancellationToken cancellationToken)
    {
        var setting = await dbContext.Settings
            .FirstOrDefaultAsync(s => s.Key == SettingKey, cancellationToken);

        string jsonValue = JsonSerializer.Serialize(request.Settings);

        if (setting == null)
        {
            setting = new Domain.Entities.Setting
            {
                Key = SettingKey,
                Group = "Payment",
                Value = jsonValue,
                UpdatedAt = DateTime.UtcNow
            };
            dbContext.Settings.Add(setting);
        }
        else
        {
            setting.Value = jsonValue;
            setting.UpdatedAt = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        cache.Remove($"settings:{tenantContext.TenantId}");
        return true;
    }
}
