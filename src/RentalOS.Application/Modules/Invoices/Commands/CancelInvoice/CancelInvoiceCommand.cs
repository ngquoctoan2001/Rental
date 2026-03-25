using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Invoices.Commands.CancelInvoice;

public record CancelInvoiceCommand : IRequest<Result<Unit>>
{
    public Guid InvoiceId { get; init; }
    public string? Reason { get; init; }
}

public class CancelInvoiceCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<CancelInvoiceCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(CancelInvoiceCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<Unit>.Fail("INVOICE_NOT_FOUND", "Không tìm thấy hóa đơn.");
        
        if (invoice.Status == InvoiceStatus.Paid)
            return Result<Unit>.Fail("ALREADY_PAID", "Không thể hủy hóa đơn đã thanh toán.");

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.Notes = $"[Hủy: {request.Reason}] " + invoice.Notes;

        context.AuditLogs.Add(new AuditLog
        {
            Action = "invoices.cancel",
            EntityType = "Invoice",
            EntityId = invoice.Id,
            EntityCode = invoice.InvoiceCode,
            UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
            NewValue = JsonSerializer.Serialize(new { request.Reason })
        });

        await context.SaveChangesAsync(cancellationToken);
        return Result<Unit>.Ok(Unit.Value);
    }
}
