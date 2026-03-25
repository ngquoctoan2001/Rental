using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using System.Text.Json;

namespace RentalOS.Application.Modules.Invoices.Commands.CreateInvoice;

public record CreateInvoiceCommand : IRequest<Result<Guid>>
{
    public Guid ContractId { get; init; }
    public DateOnly BillingMonth { get; init; }
    public int ElectricityOld { get; init; }
    public int ElectricityNew { get; init; }
    public int WaterOld { get; init; }
    public int WaterNew { get; init; }
    public decimal InternetFee { get; init; }
    public decimal GarbageFee { get; init; }
    public decimal OtherFees { get; init; }
    public string? OtherFeesNote { get; init; }
    public decimal Discount { get; init; }
    public string? DiscountNote { get; init; }
    public string? Notes { get; init; }
    public string? MeterImageElectricity { get; init; }
    public string? MeterImageWater { get; init; }
}

public class CreateInvoiceCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<CreateInvoiceCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInvoiceCommand request, CancellationToken cancellationToken)
    {
        // 1. Kiểm tra contract active
        var contract = await context.Contracts
            .Include(c => c.Room)
            .FirstOrDefaultAsync(c => c.Id == request.ContractId, cancellationToken);

        if (contract == null) return Result<Guid>.Fail("CONTRACT_NOT_FOUND", "Không tìm thấy hợp đồng.");
        if (contract.Status != ContractStatus.Active)
            return Result<Guid>.Fail("CONTRACT_NOT_ACTIVE", "Hợp đồng không còn hiệu lực.");

        // 2. Kiểm tra chưa có invoice cho tháng này (billing_month là ngày 1 của tháng)
        var firstDayOfMonth = new DateOnly(request.BillingMonth.Year, request.BillingMonth.Month, 1);
        var existingInvoice = await context.Invoices
            .AnyAsync(i => i.ContractId == request.ContractId && i.BillingMonth == firstDayOfMonth, cancellationToken);

        if (existingInvoice)
            return Result<Guid>.Fail("INVOICE_EXISTS", $"Hợp đồng đã có hóa đơn cho tháng {request.BillingMonth:MM/yyyy}.");

        // 3. Tính toán
        var electricityQty = Math.Max(0, request.ElectricityNew - request.ElectricityOld);
        var waterQty = Math.Max(0, request.WaterNew - request.WaterOld);

        var electricityPrice = contract.ElectricityPrice ?? contract.Room.ElectricityPrice;
        var waterPrice = contract.WaterPrice ?? contract.Room.WaterPrice;

        var electricityAmount = electricityQty * electricityPrice;
        var waterAmount = waterQty * waterPrice;

        var totalAmount = contract.MonthlyRent
                        + electricityAmount
                        + waterAmount
                        + (request.InternetFee > 0 ? request.InternetFee : (contract.InternetFee ?? contract.Room.InternetFee))
                        + (request.GarbageFee > 0 ? request.GarbageFee : (contract.GarbageFee ?? contract.Room.GarbageFee))
                        + (contract.ServiceFee ?? contract.Room.ServiceFee)
                        + request.OtherFees
                        - request.Discount;

        // internetFee và garbageFee ưu tiên từ request, nếu không lấy từ contract/room
        var internetFee = request.InternetFee > 0 ? request.InternetFee : (contract.InternetFee ?? contract.Room.InternetFee);
        var garbageFee = request.GarbageFee > 0 ? request.GarbageFee : (contract.GarbageFee ?? contract.Room.GarbageFee);
        var serviceFee = contract.ServiceFee ?? contract.Room.ServiceFee;

        // 4. Generate invoice_code: INV-{YYYY}-{MM}-{seq}
        var year = firstDayOfMonth.Year;
        var month = firstDayOfMonth.Month;
        var sequence = await context.Invoices
            .CountAsync(i => i.BillingMonth.Year == year && i.BillingMonth.Month == month, cancellationToken);
        var invoiceCode = $"INV-{year}-{month:00}-{(sequence + 1).ToString().PadLeft(3, '0')}";

        // 5. Generate payment_link_token: GUID format "N"
        var paymentLinkToken = Guid.NewGuid().ToString("N");

        // 7. Set due_date = billing_month + billing_date days
        var dueDate = firstDayOfMonth.AddDays(contract.BillingDate - 1); 
        // Nếu BillingMonth là ngày 1, và billingDate là 5, thì dueDate là ngày 5.

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            ContractId = contract.Id,
            InvoiceCode = invoiceCode,
            BillingMonth = firstDayOfMonth,
            DueDate = dueDate,
            ElectricityOld = request.ElectricityOld,
            ElectricityNew = request.ElectricityNew,
            ElectricityPrice = electricityPrice,
            ElectricityAmount = electricityAmount,
            WaterOld = request.WaterOld,
            WaterNew = request.WaterNew,
            WaterPrice = waterPrice,
            WaterAmount = waterAmount,
            RoomRent = contract.MonthlyRent,
            ServiceFee = serviceFee,
            InternetFee = internetFee,
            GarbageFee = garbageFee,
            OtherFees = request.OtherFees,
            OtherFeesNote = request.OtherFeesNote,
            Discount = request.Discount,
            DiscountNote = request.DiscountNote,
            TotalAmount = totalAmount,
            Status = InvoiceStatus.Pending,
            PaymentLinkToken = paymentLinkToken,
            PaymentLinkExpiresAt = DateTime.UtcNow.AddDays(30),
            Notes = request.Notes,
            MeterImageElectricity = request.MeterImageElectricity,
            MeterImageWater = request.MeterImageWater,
            CreatedBy = Guid.TryParse(currentUserService.UserId, out var creatorId) ? creatorId : null
        };

        context.Invoices.Add(invoice);

        // Audit Log
        var auditLog = new AuditLog
        {
            Action = "invoices.create",
            EntityType = "Invoice",
            EntityId = invoice.Id,
            EntityCode = invoice.InvoiceCode,
            UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
            NewValue = JsonSerializer.Serialize(invoice)
        };
        context.AuditLogs.Add(auditLog);

        await context.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Ok(invoice.Id);
    }
}
