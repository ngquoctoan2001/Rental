using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Payments.Commands.CreateMoMoPayment;

public record CreateMoMoPaymentCommand(Guid InvoiceId, string ReturnUrl, string NotifyUrl) : IRequest<Result<MoMoPaymentResult>>;

public class CreateMoMoPaymentCommandHandler(
    IApplicationDbContext context,
    IMoMoService momoService) : IRequestHandler<CreateMoMoPaymentCommand, Result<MoMoPaymentResult>>
{
    public async Task<Result<MoMoPaymentResult>> Handle(CreateMoMoPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<MoMoPaymentResult>.Fail("NOT_FOUND", "Không tìm thấy hóa đơn.");

        try
        {
            var result = await momoService.CreatePaymentAsync(invoice, request.ReturnUrl, request.NotifyUrl);
            
            // Log interaction
            context.PaymentLinkLogs.Add(new PaymentLinkLog
            {
                InvoiceId = invoice.Id,
                Action = "momo_initiated",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);

            return Result<MoMoPaymentResult>.Ok(result);
        }
        catch (Exception ex)
        {
            return Result<MoMoPaymentResult>.Fail("MOMO_ERROR", ex.Message);
        }
    }
}
