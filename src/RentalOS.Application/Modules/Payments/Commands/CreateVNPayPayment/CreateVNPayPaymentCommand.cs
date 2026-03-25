using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Payments.Commands.CreateVNPayPayment;

public record CreateVNPayPaymentCommand(Guid InvoiceId, string ReturnUrl) : IRequest<Result<string>>;

public class CreateVNPayPaymentCommandHandler(
    IApplicationDbContext context,
    IVNPayService vnpayService) : IRequestHandler<CreateVNPayPaymentCommand, Result<string>>
{
    public async Task<Result<string>> Handle(CreateVNPayPaymentCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<string>.Fail("NOT_FOUND", "Không tìm thấy hóa đơn.");

        try
        {
            var url = await vnpayService.CreatePaymentAsync(invoice, request.ReturnUrl);
            
            // Log interaction
            context.PaymentLinkLogs.Add(new PaymentLinkLog
            {
                InvoiceId = invoice.Id,
                Action = "vnpay_initiated",
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync(cancellationToken);

            return Result<string>.Ok(url);
        }
        catch (Exception ex)
        {
            return Result<string>.Fail("VNPAY_ERROR", ex.Message);
        }
    }
}
