using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using System.Text.Json;

namespace RentalOS.Infrastructure.BackgroundJobs;

public class SendInvoiceNotificationJob(
    IApplicationDbContext context,
    IInvoicePdfService pdfService)
{
    public async Task Execute(Guid invoiceId, string channel)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
                .ThenInclude(c => c.Customer)
            .Include(i => i.Contract)
                .ThenInclude(c => c.Room)
            .FirstOrDefaultAsync(i => i.Id == invoiceId);

        if (invoice == null) return;

        // 1. Tạo PDF nếu chưa có
        if (string.IsNullOrEmpty(invoice.PdfUrl))
        {
            await pdfService.GenerateAndUploadInvoicePdfAsync(invoiceId);
        }

        // 2. Giả lập gửi tin nhắn qua Zalo/SMS/Email
        string message = $"RentalOS: Hóa đơn tháng {invoice.BillingMonth:MM/yyyy} của phòng {invoice.Contract.Room.RoomNumber} đã sẵn sàng. " +
                         $"Số tiền: {invoice.TotalAmount:N0} VNĐ. " +
                         $"Link thanh toán: https://rentalos.vn/pay/{invoice.PaymentLinkToken}";

        Console.WriteLine($"[NOTIFY] Gửi qua {channel} cho {invoice.Contract.Customer.FullName} ({invoice.Contract.Customer.Phone}): {message}");

        // 3. Log Audit
        context.AuditLogs.Add(new AuditLog
        {
            Action = "notifications.invoice_sent",
            EntityType = "Invoice",
            EntityId = invoice.Id,
            UserId = null,
            NewValue = JsonSerializer.Serialize(new { channel, message })
        });

        await context.SaveChangesAsync(CancellationToken.None);
    }
}
