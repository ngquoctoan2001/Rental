using Polly;
using Polly.Retry;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace RentalOS.Infrastructure.Services.Notifications;

public class NotificationService : INotificationService
{
    private readonly IZaloService _zaloService;
    private readonly ISmsService _smsService;
    private readonly IEmailService _emailService;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<NotificationService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public NotificationService(
        IZaloService zaloService,
        ISmsService smsService,
        IEmailService emailService,
        ITenantContext tenantContext,
        ILogger<NotificationService> logger)
    {
        _zaloService = zaloService;
        _smsService = smsService;
        _emailService = emailService;
        _tenantContext = tenantContext;
        _logger = logger;

        // Exponential backoff: 1s, 3s, 9s
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(3, retryAttempt - 1)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning("Retry {RetryCount} for notification after {TimeSpan} due to {Message}", 
                        retryCount, timeSpan, exception.Message);
                });
    }

    public async Task SendInvoiceCreatedAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, string paymentLink)
    {
        var message = $@"🏠 HÓA ĐƠN THÁNG {invoice.Month}
━━━━━━━━━━━━━━━━
Phòng: {invoice.RoomNumber} - {invoice.PropertyName}
Tiền phòng:  {invoice.RoomRent:N0}đ
Tiền điện:   {invoice.ElectricityAmount:N0}đ ({invoice.ElectricityUsed} kWh)
Tiền nước:   {invoice.WaterAmount:N0}đ ({invoice.WaterUsed} m³)
Phí DV:      {invoice.ServiceFee:N0}đ
━━━━━━━━━━━━━━━━
TỔNG CỘNG: {invoice.TotalAmount:N0}đ
Hạn TT: {invoice.DueDate:dd/MM/yyyy}

💳 Thanh toán: {paymentLink}";

        await ExecuteWithFallback(customer, message, "invoice_created");
    }

    public async Task SendInvoiceReminderAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, int daysUntilDue)
    {
        var message = $"Nhắc nhở: Hóa đơn {invoice.InvoiceCode} ({invoice.TotalAmount:N0}đ) đến hạn {invoice.DueDate:dd/MM/yyyy}. Link: ...";
        await ExecuteWithFallback(customer, message, "invoice_reminder");
    }

    public async Task SendInvoiceOverdueAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, int daysOverdue)
    {
        var message = $"⚠ Hóa đơn {invoice.InvoiceCode} đã quá hạn {daysOverdue} ngày. Vui lòng thanh toán sớm.";
        await ExecuteWithFallback(customer, message, "invoice_overdue");
    }

    public async Task SendPaymentConfirmedAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, decimal amount)
    {
        var message = $"✅ Đã nhận thanh toán {amount:N0}đ cho hóa đơn {invoice.InvoiceCode}. Cảm ơn bạn!";
        await _retryPolicy.ExecuteAsync(() => _zaloService.SendMessageAsync(customer.Phone, message, _tenantContext.TenantId));
    }

    public async Task SendContractExpiryAlertAsync(object contract, string ownerEmail, int daysUntilExpiry)
    {
        var subject = $"Hợp đồng sắp hết hạn ({daysUntilExpiry} ngày)";
        var body = "Hợp đồng ... sắp hết hạn. Vui lòng kiểm tra.";
        await _retryPolicy.ExecuteAsync(() => _emailService.SendEmailAsync(ownerEmail, subject, body, _tenantContext.TenantId));
    }

    public async Task SendMonthlyReportAsync(string tenantEmail, object reportData)
    {
        var subject = "Báo cáo doanh thu tháng";
        var body = "Đính kèm báo cáo tháng...";
        await _retryPolicy.ExecuteAsync(() => _emailService.SendEmailAsync(tenantEmail, subject, body, _tenantContext.TenantId));
    }

    private async Task ExecuteWithFallback(NotificationCustomerDto customer, string message, string eventType)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            var success = await _zaloService.SendMessageAsync(customer.Phone, message, _tenantContext.TenantId);
            if (!success)
            {
                _logger.LogInformation("Zalo failed, falling back to SMS for {Phone}", customer.Phone);
                await _smsService.SendSmsAsync(customer.Phone, message, _tenantContext.TenantId);
            }
        });
    }
}
