using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using System.Text.Json;

namespace RentalOS.Application.Modules.Payments.Queries.GetPaymentSettings;

public record GetPaymentSettingsQuery() : IRequest<Result<PaymentSettingsDto>>;

public class GetPaymentSettingsQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetPaymentSettingsQuery, Result<PaymentSettingsDto>>
{
    public async Task<Result<PaymentSettingsDto>> Handle(GetPaymentSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await context.Settings
            .Where(s => s.Group == "payment")
            .ToListAsync(cancellationToken);

        var dto = new PaymentSettingsDto();

        var momo = settings.FirstOrDefault(s => s.Key == "payment.momo");
        if (momo != null) dto.MoMo = JsonSerializer.Deserialize<MoMoOptions>(momo.Value);

        var vnpay = settings.FirstOrDefault(s => s.Key == "payment.vnpay");
        if (vnpay != null) dto.VNPay = JsonSerializer.Deserialize<VNPayOptions>(vnpay.Value);

        var bank = settings.FirstOrDefault(s => s.Key == "payment.bank_transfer");
        if (bank != null) dto.BankTransfer = JsonSerializer.Deserialize<BankTransferOptions>(bank.Value);

        return Result<PaymentSettingsDto>.Ok(dto);
    }
}

public class PaymentSettingsDto
{
    public MoMoOptions? MoMo { get; set; }
    public VNPayOptions? VNPay { get; set; }
    public BankTransferOptions? BankTransfer { get; set; }
}
