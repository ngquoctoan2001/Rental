using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using StackExchange.Redis;
using System.Text.Json;

namespace RentalOS.Infrastructure.Services.Payments;

public class VNPayService(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IConnectionMultiplexer redis,
    IBackgroundJobService backgroundJobService,
    ILogger<VNPayService> logger) : IVNPayService
{
    public async Task<string> CreatePaymentAsync(Invoice invoice, string returnUrl)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == "payment.vnpay");
        if (setting == null) throw new Exception("VNPay chưa được cấu hình cho tenant này.");

        var options = JsonSerializer.Deserialize<VNPayOptions>(setting.Value)
            ?? throw new Exception("Cấu hình VNPay không hợp lệ.");

        string vnp_Version = "2.1.0";
        string vnp_Command = "pay";
        string vnp_TmnCode = options.TmnCode;
        string vnp_Amount = ((long)invoice.TotalAmount * 100).ToString(); // VNPay unit is cents (VND * 100)
        string vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");
        string vnp_CurrCode = "VND";
        string vnp_IpAddr = "127.0.0.1"; // Should be passed from request
        string vnp_Locale = "vn";
        string vnp_OrderInfo = $"Thanh toan hoa don {invoice.InvoiceCode}";
        string vnp_OrderType = "other";
        string vnp_ReturnUrl = returnUrl;
        string vnp_TxnRef = $"{tenantContext.TenantSlug}-{invoice.InvoiceCode}-{vnp_CreateDate}";

        var vnpayData = new SortedList<string, string>
        {
            { "vnp_Version", vnp_Version },
            { "vnp_Command", vnp_Command },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", vnp_Amount },
            { "vnp_CurrCode", vnp_CurrCode },
            { "vnp_TxnRef", vnp_TxnRef },
            { "vnp_OrderInfo", vnp_OrderInfo },
            { "vnp_OrderType", vnp_OrderType },
            { "vnp_ReturnUrl", vnp_ReturnUrl },
            { "vnp_IpAddr", vnp_IpAddr },
            { "vnp_CreateDate", vnp_CreateDate },
            { "vnp_Locale", vnp_Locale }
        };

        StringBuilder data = new StringBuilder();
        foreach (KeyValuePair<string, string> kv in vnpayData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        string queryString = data.ToString().Remove(data.Length - 1);
        string vnp_SecureHash = SignHmacSha512(queryString, options.HashSecret);
        string paymentUrl = $"{options.BaseUrl}?{queryString}&vnp_SecureHash={vnp_SecureHash}";

        return paymentUrl;
    }

    public async Task<VNPayWebhookResult> ProcessWebhookAsync(IDictionary<string, string> queryParams)
    {
        if (!queryParams.TryGetValue("vnp_SecureHash", out var vnp_SecureHash))
            return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "Missing signature" };

        string vnp_TxnRef = queryParams["vnp_TxnRef"];
        string vnp_ResponseCode = queryParams["vnp_ResponseCode"];
        string vnp_TransactionNo = queryParams["vnp_TransactionNo"];
        long vnp_Amount = long.Parse(queryParams["vnp_Amount"]) / 100;

        // Idempotency check
        var db = redis.GetDatabase();
        var redisKey = $"vnpay_webhook_{vnp_TxnRef}";
        if (await db.KeyExistsAsync(redisKey))
        {
            return new VNPayWebhookResult { IsSuccess = true, AlreadyProcessed = true };
        }

        // 1. Resolve Tenant
        var parts = vnp_TxnRef.Split('-');
        if (parts.Length < 3) return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "vnp_TxnRef invalid" };
        string tenantSlug = parts[0];
        string invoiceCode = parts[1];

        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug);
        if (tenant == null) return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "Tenant not found" };

        tenantContext.Initialize(tenant.Slug, tenant.SchemaName, Guid.Empty, UserRole.TenantAdmin, tenant.Plan);

        // 2. Verify Signature
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == "payment.vnpay");
        if (setting == null) return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "VNPay not configured" };
        var options = JsonSerializer.Deserialize<VNPayOptions>(setting.Value);
        if (options == null) return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "Invalid config" };

        StringBuilder data = new StringBuilder();
        foreach (var kv in queryParams.OrderBy(q => q.Key))
        {
            if (!string.IsNullOrEmpty(kv.Value) && kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash")
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }
        string rawData = data.ToString().Remove(data.Length - 1);
        string expectedHash = SignHmacSha512(rawData, options.HashSecret);

        if (vnp_SecureHash.ToUpper() != expectedHash.ToUpper())
        {
            return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "Invalid signature" };
        }

        // 3. Business Logic
        if (vnp_ResponseCode != "00")
        {
            logger.LogWarning("VNPay Payment failed for {Ref}: {Code}", vnp_TxnRef, vnp_ResponseCode);
            return new VNPayWebhookResult { IsSuccess = true };
        }

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceCode == invoiceCode);
        if (invoice == null) return new VNPayWebhookResult { IsSuccess = false, ErrorMessage = "Invoice not found" };

        if (invoice.Status != InvoiceStatus.Paid)
        {
            var transaction = new Transaction
            {
                InvoiceId = invoice.Id,
                Amount = vnp_Amount,
                Method = TransactionMethod.VNPay,
                Direction = TransactionDirection.Income,
                Category = TransactionCategory.Rent,
                ProviderRef = vnp_TransactionNo,
                ProviderResponse = JsonSerializer.Serialize(queryParams),
                Status = TransactionStatus.Success,
                Note = $"Thanh toán VNPay cho {invoiceCode}",
                PaidAt = DateTime.UtcNow
            };
            context.Transactions.Add(transaction);

            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            await context.SaveChangesAsync(CancellationToken.None);
            
            await db.StringSetAsync(redisKey, "processed", TimeSpan.FromDays(1));
        }

        return new VNPayWebhookResult { IsSuccess = true };
    }

    public Task<VNPayReturnResult> ProcessReturnAsync(IDictionary<string, string> queryParams)
    {
        string vnp_ResponseCode = queryParams["vnp_ResponseCode"];
        string vnp_TxnRef = queryParams["vnp_TxnRef"];
        var parts = vnp_TxnRef.Split('-');
        
        return Task.FromResult(new VNPayReturnResult
        {
            IsSuccess = vnp_ResponseCode == "00",
            InvoiceCode = parts.Length > 1 ? parts[1] : null
        });
    }

    private string SignHmacSha512(string message, string key)
    {
        byte[] keyByte = Encoding.UTF8.GetBytes(key);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmac = new HMACSHA512(keyByte);
        byte[] hash = hmac.ComputeHash(messageBytes);
        return BitConverter.ToString(hash).Replace("-", "").ToUpper();
    }
}
