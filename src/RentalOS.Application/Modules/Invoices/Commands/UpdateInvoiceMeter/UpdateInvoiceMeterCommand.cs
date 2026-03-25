using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Invoices.Commands.UpdateInvoiceMeter;

public record UpdateInvoiceMeterCommand : IRequest<Result<Unit>>
{
    public Guid InvoiceId { get; init; }
    public int ElectricityNew { get; init; }
    public int WaterNew { get; init; }
    public string? MeterImageElectricity { get; init; }
    public string? MeterImageWater { get; init; }
}

public class UpdateInvoiceMeterCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<UpdateInvoiceMeterCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdateInvoiceMeterCommand request, CancellationToken cancellationToken)
    {
        var invoice = await context.Invoices
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

        if (invoice == null) return Result<Unit>.Fail("INVOICE_NOT_FOUND", "Không tìm thấy hóa đơn.");
        
        // Chỉ cho phép cập nhật nếu chưa thanh toán hoặc chưa bị hủy
        if (invoice.Status == InvoiceStatus.Paid || invoice.Status == InvoiceStatus.Cancelled)
            return Result<Unit>.Fail("INVALID_STATUS", "Không thể cập nhật chỉ số cho hóa đơn đã thanh toán hoặc đã hủy.");

        // Cập nhật chỉ số
        invoice.ElectricityNew = request.ElectricityNew;
        invoice.WaterNew = request.WaterNew;
        
        if (!string.IsNullOrEmpty(request.MeterImageElectricity))
            invoice.MeterImageElectricity = request.MeterImageElectricity;
            
        if (!string.IsNullOrEmpty(request.MeterImageWater))
            invoice.MeterImageWater = request.MeterImageWater;

        // Tính toán lại các khoản phí dựa trên chỉ số mới
        var electricityQty = Math.Max(0, invoice.ElectricityNew - invoice.ElectricityOld);
        var waterQty = Math.Max(0, invoice.WaterNew - invoice.WaterOld);
        
        invoice.ElectricityAmount = electricityQty * invoice.ElectricityPrice;
        invoice.WaterAmount = waterQty * invoice.WaterPrice;
        
        invoice.TotalAmount = invoice.RoomRent
                            + invoice.ElectricityAmount
                            + invoice.WaterAmount
                            + invoice.ServiceFee
                            + invoice.InternetFee
                            + invoice.GarbageFee
                            + invoice.OtherFees
                            - invoice.Discount;

        // Audit Log
        context.AuditLogs.Add(new AuditLog
        {
            Action = "invoices.update_meter",
            EntityType = "Invoice",
            EntityId = invoice.Id,
            EntityCode = invoice.InvoiceCode,
            UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
            NewValue = JsonSerializer.Serialize(new { 
                request.ElectricityNew, 
                request.WaterNew, 
                invoice.ElectricityAmount, 
                invoice.WaterAmount, 
                invoice.TotalAmount 
            })
        });

        await context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Ok(Unit.Value);
    }
}
