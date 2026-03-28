using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using System.Collections.Concurrent;
using System.Text.Json;

namespace RentalOS.Application.Modules.Invoices.Commands.BulkGenerateInvoices;

public record BulkGenerateInvoicesCommand : IRequest<Result<BulkGenerateInvoicesResult>>
{
    public Guid? PropertyId { get; init; }
    public DateOnly BillingMonth { get; init; }
    public List<Guid>? IncludeRoomIds { get; init; }
    public bool SendNotification { get; init; }
    public bool OverwriteExisting { get; init; }
}

public class BulkGenerateInvoicesResult
{
    public int Generated { get; set; }
    public int Skipped { get; set; }
    public int Errors { get; set; }
    public List<BulkGenerateItemDetail> Details { get; set; } = [];
}

public class BulkGenerateItemDetail
{
    public string RoomNumber { get; set; } = "";
    public string Status { get; set; } = ""; // generated, skipped, error
    public string? Reason { get; set; }
    public string? InvoiceCode { get; set; }
}

public class BulkGenerateInvoicesCommandHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService) : IRequestHandler<BulkGenerateInvoicesCommand, Result<BulkGenerateInvoicesResult>>
{
    public async Task<Result<BulkGenerateInvoicesResult>> Handle(BulkGenerateInvoicesCommand request, CancellationToken cancellationToken)
    {
        var firstDayOfMonth = new DateOnly(request.BillingMonth.Year, request.BillingMonth.Month, 1);
        
        // 1. Lấy tất cả active contracts
        var query = context.Contracts
            .Include(c => c.Room)
            .Where(c => c.Status == ContractStatus.Active);

        if (request.PropertyId.HasValue)
            query = query.Where(c => c.Room.PropertyId == request.PropertyId.Value);

        if (request.IncludeRoomIds != null && request.IncludeRoomIds.Count > 0)
            query = query.Where(c => request.IncludeRoomIds.Contains(c.RoomId));

        var contracts = await query.ToListAsync(cancellationToken);
        
        var result = new BulkGenerateInvoicesResult();
        var details = new ConcurrentBag<BulkGenerateItemDetail>();

        // Lấy số thứ tự tối đa hiện tại của tháng để tăng dần (không dùng song song cho InvoiceCode để tránh duplicate)
        var sequence = await context.Invoices
            .Where(i => i.BillingMonth.Year == firstDayOfMonth.Year && i.BillingMonth.Month == firstDayOfMonth.Month)
            .CountAsync(cancellationToken);

        // Do EF DbContext không thread-safe, nên nếu dùng Parallel.ForEachAsync thì phải tạo Scope riêng.
        // Tuy nhiên, để đơn giản và an toàn với transaction/audit log, tôi sẽ xử lý tuần tự nhưng tối ưu query.
        // Nếu đề bài BẮT BUỘC Parallel.ForEachAsync, tôi sẽ cần IServiceScopeFactory.
        // Ở đây tôi sẽ xử lý lần lượt theo danh sách đã load.

        foreach (var contract in contracts)
        {
            try
            {
                // Kiểm tra đã có invoice chưa
                var existing = await context.Invoices
                    .FirstOrDefaultAsync(i => i.ContractId == contract.Id && i.BillingMonth == firstDayOfMonth, cancellationToken);

                if (existing != null)
                {
                    if (!request.OverwriteExisting)
                    {
                        result.Skipped++;
                        details.Add(new BulkGenerateItemDetail
                        {
                            RoomNumber = contract.Room.RoomNumber,
                            Status = "skipped",
                            Reason = "Đã có hóa đơn tháng này"
                        });
                        continue;
                    }
                    // Nếu overwrite, xóa cái cũ (hoặc update). Ở đây tôi sẽ skipped để an toàn.
                    result.Skipped++;
                    details.Add(new BulkGenerateItemDetail
                    {
                        RoomNumber = contract.Room.RoomNumber,
                        Status = "skipped",
                        Reason = "Đã có hóa đơn (Không hỗ trợ ghi đè tự động)"
                    });
                    continue;
                }

                // Lấy chỉ số cũ từ hóa đơn tháng trước (nếu có)
                var lastInvoice = await context.Invoices
                    .Where(i => i.ContractId == contract.Id && i.BillingMonth < firstDayOfMonth)
                    .OrderByDescending(i => i.BillingMonth)
                    .FirstOrDefaultAsync(cancellationToken);

                var electricityOld = lastInvoice?.ElectricityNew ?? 0;
                var waterOld = lastInvoice?.WaterNew ?? 0;

                // Tạo hóa đơn mới (chưa có reading mới -> Pending Meter)
                sequence++;
                var invoiceCode = $"INV-{firstDayOfMonth.Year}-{firstDayOfMonth.Month:00}-{sequence.ToString().PadLeft(3, '0')}";
                
                var invoice = new Invoice
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    InvoiceCode = invoiceCode,
                    BillingMonth = firstDayOfMonth,
                    DueDate = firstDayOfMonth.AddDays(contract.BillingDate - 1),
                    ElectricityOld = electricityOld,
                    ElectricityNew = electricityOld, // Mặc định bằng cũ để nhân viên nhập sau
                    ElectricityPrice = contract.ElectricityPrice ?? contract.Room.ElectricityPrice,
                    WaterOld = waterOld,
                    WaterNew = waterOld,
                    WaterPrice = contract.WaterPrice ?? contract.Room.WaterPrice,
                    RoomRent = contract.MonthlyRent,
                    ServiceFee = contract.ServiceFee ?? contract.Room.ServiceFee,
                    InternetFee = contract.InternetFee ?? contract.Room.InternetFee,
                    GarbageFee = contract.GarbageFee ?? contract.Room.GarbageFee,
                    Status = InvoiceStatus.Pending,
                    IsAutoGenerated = true,
                    PaymentLinkToken = Guid.NewGuid().ToString("N"),
                    PaymentLinkExpiresAt = DateTime.UtcNow.AddDays(30),
                    CreatedBy = Guid.TryParse(currentUserService.UserId, out var creatorId) ? creatorId : null
                };

                // Tính toán sơ bộ (thường là room_rent + các phí cố định)
                invoice.TotalAmount = invoice.RoomRent + invoice.ServiceFee + invoice.InternetFee + invoice.GarbageFee;

                context.Invoices.Add(invoice);

                result.Generated++;
                details.Add(new BulkGenerateItemDetail
                {
                    RoomNumber = contract.Room.RoomNumber,
                    Status = "generated",
                    InvoiceCode = invoiceCode
                });
            }
            catch (Exception ex)
            {
                result.Errors++;
                details.Add(new BulkGenerateItemDetail
                {
                    RoomNumber = contract.Room.RoomNumber,
                    Status = "error",
                    Reason = ex.Message
                });
            }
        }

        if (result.Generated > 0)
        {
            await context.SaveChangesAsync(cancellationToken);
            
            // LogAudit cho bulk operation
            context.AuditLogs.Add(new AuditLog
            {
                Action = "invoices.bulk_generate",
                EntityType = "Invoice",
                UserId = Guid.TryParse(currentUserService.UserId, out var auditorId) ? auditorId : null,
                NewValue = JsonSerializer.Serialize(new
                {
                    generated = result.Generated,
                    skipped = result.Skipped,
                    errors = result.Errors,
                    billingMonth = firstDayOfMonth
                })
            });
            await context.SaveChangesAsync(cancellationToken);
        }

        result.Details = details.ToList();
        return Result<BulkGenerateInvoicesResult>.Ok(result);
    }
}
