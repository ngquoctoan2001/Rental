using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Invoices.Queries.GetInvoiceByPaymentToken;

public record GetInvoiceByPaymentTokenQuery(string Token) : IRequest<Result<InvoiceDto>>;

public class GetInvoiceByPaymentTokenQueryHandler(IApplicationDbContext context) 
    : IRequestHandler<GetInvoiceByPaymentTokenQuery, Result<InvoiceDto>>
{
    public async Task<Result<InvoiceDto>> Handle(GetInvoiceByPaymentTokenQuery request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
                .ThenInclude(c => c.Customer)
            .Include(i => i.Contract)
                .ThenInclude(c => c.Room)
                    .ThenInclude(r => r.Property)
            .AsNoTracking()
            .Where(i => i.PaymentLinkToken == request.Token)
            .Select(i => new InvoiceDto
            {
                Id = i.Id,
                InvoiceCode = i.InvoiceCode,
                ContractId = i.ContractId,
                CustomerName = i.Contract.Customer.FullName,
                RoomNumber = i.Contract.Room.RoomNumber,
                PropertyName = i.Contract.Room.Property.Name,
                BillingMonth = i.BillingMonth,
                DueDate = i.DueDate,
                TotalAmount = i.TotalAmount,
                Status = i.Status,
                PartialPaidAmount = i.PartialPaidAmount,
                PaymentLinkToken = i.PaymentLinkToken,
                SentAt = i.SentAt,
                PaidAt = i.PaidAt,
                PdfUrl = i.PdfUrl,
                ElectricityOld = i.ElectricityOld,
                ElectricityNew = i.ElectricityNew,
                ElectricityPrice = i.ElectricityPrice,
                ElectricityAmount = i.ElectricityAmount,
                WaterOld = i.WaterOld,
                WaterNew = i.WaterNew,
                WaterPrice = i.WaterPrice,
                WaterAmount = i.WaterAmount,
                RoomRent = i.RoomRent,
                ServiceFee = i.ServiceFee,
                InternetFee = i.InternetFee,
                GarbageFee = i.GarbageFee,
                OtherFees = i.OtherFees,
                OtherFeesNote = i.OtherFeesNote,
                Discount = i.Discount,
                DiscountNote = i.DiscountNote,
                Notes = i.Notes
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (invoice == null) return Result<InvoiceDto>.Fail("TOKEN_INVALID", "Mã thanh toán không hợp lệ hoặc đã hết hạn.");

        // Log the view (optional: get IP/UA from a context if available, but for now just simple log)
        context.PaymentLinkLogs.Add(new RentalOS.Domain.Entities.PaymentLinkLog
        {
            InvoiceId = invoice.Id,
            Action = "viewed",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync(cancellationToken);

        return Result<InvoiceDto>.Ok(invoice);
    }
}
