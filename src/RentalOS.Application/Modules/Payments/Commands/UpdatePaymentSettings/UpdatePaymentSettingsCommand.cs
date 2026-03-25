using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using System.Text.Json;

namespace RentalOS.Application.Modules.Payments.Commands.UpdatePaymentSettings;

public record UpdatePaymentSettingsCommand : IRequest<Result<bool>>
{
    public MoMoOptions? MoMo { get; set; }
    public VNPayOptions? VNPay { get; set; }
    public BankTransferOptions? BankTransfer { get; set; }
}

public class UpdatePaymentSettingsCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<UpdatePaymentSettingsCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(UpdatePaymentSettingsCommand request, CancellationToken cancellationToken)
    {
        var settings = await context.Settings
            .Where(s => s.Group == "payment")
            .ToListAsync(cancellationToken);

        await UpdateSetting("payment.momo", request.MoMo, settings, cancellationToken);
        await UpdateSetting("payment.vnpay", request.VNPay, settings, cancellationToken);
        await UpdateSetting("payment.bank_transfer", request.BankTransfer, settings, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
        return Result<bool>.Ok(true);
    }

    private async Task UpdateSetting<T>(string key, T? value, List<Setting> existingSettings, CancellationToken ct)
    {
        if (value == null) return;

        var setting = existingSettings.FirstOrDefault(s => s.Key == key);
        var json = JsonSerializer.Serialize(value);

        if (setting == null)
        {
            context.Settings.Add(new Setting
            {
                Key = key,
                Group = "payment",
                Value = json,
                UpdatedAt = DateTime.UtcNow,
                UpdatedBy = Guid.TryParse(currentUserService.UserId, out var updaterId) ? updaterId : null
            });
        }
        else
        {
            setting.Value = json;
            setting.UpdatedAt = DateTime.UtcNow;
            setting.UpdatedBy = Guid.TryParse(currentUserService.UserId, out var updaterId) ? updaterId : null;
        }
    }
}
