using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Enums;
using RentalOS.Application.Common.BackgroundJobs;

namespace RentalOS.Application.Modules.Invoices.Commands.SendInvoice;

public record SendInvoiceCommand : IRequest<Result<Unit>>
{
    public Guid InvoiceId { get; init; }
    public string Channel { get; init; } = "zalo"; // zalo | sms | both | email
}

public class SendInvoiceCommandHandler(
    IApplicationDbContext context,
    IBackgroundJobService backgroundJobService) : IRequestHandler<SendInvoiceCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(SendInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
            .ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<Unit>.Fail("INVOICE_NOT_FOUND", "Không tìm thấy hóa đơn.");
        
        if (invoice.Status == InvoiceStatus.Cancelled)
            return Result<Unit>.Fail("INVOICE_CANCELLED", "Không thể gửi hóa đơn đã bị hủy.");

        // Enqueue Hangfire job
        backgroundJobService.Enqueue<SendInvoiceNotificationJob>(j => j.Execute(invoice.Id, request.Channel));

        // Cập nhật ngày gửi
        invoice.SentAt = DateTime.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
