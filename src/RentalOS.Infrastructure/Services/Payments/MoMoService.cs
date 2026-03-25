using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;
using RentalOS.Domain.Enums;
using StackExchange.Redis;

namespace RentalOS.Infrastructure.Services.Payments;

public class MoMoService(
    IApplicationDbContext context,
    ITenantContext tenantContext,
    IHttpClientFactory httpClientFactory,
    IConnectionMultiplexer redis,
    IBackgroundJobService backgroundJobService,
    ILogger<MoMoService> logger) : IMoMoService
{
    private const string MoMoSandboxUrl = "https://test-payment.momo.vn/v2/gateway/api/create";
    private const string MoMoProductionUrl = "https://payment.momo.vn/v2/gateway/api/create";

    public async Task<MoMoPaymentResult> CreatePaymentAsync(Invoice invoice, string returnUrl, string notifyUrl)
    {
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == "payment.momo");
        if (setting == null) throw new Exception("MoMo chưa được cấu hình cho tenant này.");

        var options = JsonSerializer.Deserialize<MoMoOptions>(setting.Value)
            ?? throw new Exception("Cấu hình MoMo không hợp lệ.");

        string requestId = Guid.NewGuid().ToString();
        string orderId = $"{tenantContext.TenantSlug}-{invoice.InvoiceCode}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        string orderInfo = $"Thanh toán hóa đơn {invoice.InvoiceCode}";
        string amount = ((long)invoice.TotalAmount).ToString();
        string extraData = "";
        string requestType = "captureWallet";

        // Build raw sign string (alphabetical order)
        var rawData = $"accessKey={options.AccessKey}&amount={amount}&extraData={extraData}&ipnUrl={notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={options.PartnerCode}&requestId={requestId}&requestType={requestType}&returnUrl={returnUrl}";
        
        string signature = SignHmacSha256(rawData, options.SecretKey);

        var requestBody = new
        {
            partnerCode = options.PartnerCode,
            requestId,
            amount = long.Parse(amount),
            orderId,
            orderInfo,
            redirectUrl = returnUrl,
            ipnUrl = notifyUrl,
            requestType,
            extraData,
            lang = "vi",
            signature
        };

        var client = httpClientFactory.CreateClient();
        var url = options.IsProduction ? MoMoProductionUrl : MoMoSandboxUrl;
        
        var response = await client.PostAsJsonAsync(url, requestBody);
        var responseContent = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            logger.LogError("MoMo API Error: {Content}", responseContent);
            throw new Exception($"Lỗi khi gọi MoMo API: {response.StatusCode}");
        }

        using var doc = JsonDocument.Parse(responseContent);
        var root = doc.RootElement;
        
        if (root.GetProperty("resultCode").GetInt32() != 0)
        {
            string msg = root.GetProperty("message").GetString() ?? "Unknown error";
            logger.LogError("MoMo Business Error: {Message}", msg);
            throw new Exception($"MoMo: {msg}");
        }

        return new MoMoPaymentResult
        {
            PayUrl = root.GetProperty("payUrl").GetString() ?? "",
            QrCodeUrl = root.GetProperty("qrCodeUrl").GetString() ?? "",
            Deeplink = root.TryGetProperty("deeplink", out var dl) ? dl.GetString() ?? "" : ""
        };
    }

    public async Task<MoMoWebhookResult> ProcessWebhookAsync(string rawBody, IDictionary<string, string> headers)
    {
        using var doc = JsonDocument.Parse(rawBody);
        var root = doc.RootElement;
        
        string orderId = root.GetProperty("orderId").GetString() ?? "";
        string requestId = root.GetProperty("requestId").GetString() ?? "";
        long amount = root.GetProperty("amount").GetInt64();
        int resultCode = root.GetProperty("resultCode").GetInt32();
        string transId = root.GetProperty("transId").GetRawText(); // Handle as string to avoid precision issues
        string message = root.GetProperty("message").GetString() ?? "";
        string signature = root.GetProperty("signature").GetString() ?? "";

        // Idempotency check with Redis
        var db = redis.GetDatabase();
        var redisKey = $"momo_webhook_{orderId}";
        if (await db.KeyExistsAsync(redisKey))
        {
            return new MoMoWebhookResult { IsSuccess = true, AlreadyProcessed = true };
        }

        // 1. Resolve Tenant
        var parts = orderId.Split('-');
        if (parts.Length < 3) return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "OrderId sai định dạng" };
        string tenantSlug = parts[0];
        string invoiceCode = parts[1];

        var tenant = await context.Tenants.FirstOrDefaultAsync(t => t.Slug == tenantSlug);
        if (tenant == null) return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "Không tìm thấy tenant" };

        // Switch context to tenant schema
        tenantContext.Initialize(tenant.Slug, tenant.SchemaName, Guid.Empty, UserRole.TenantAdmin, tenant.Plan);

        // 2. Verify Signature
        var setting = await context.Settings.FirstOrDefaultAsync(s => s.Key == "payment.momo");
        if (setting == null) return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "MoMo chưa được cấu hình cho tenant" };
        var options = JsonSerializer.Deserialize<MoMoOptions>(setting.Value);
        if (options == null) return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "Cấu hình MoMo không hợp lệ" };

        // MoMo IPN signature string order
        string responseTime = root.GetProperty("responseTime").GetRawText();
        string extraData = root.GetProperty("extraData").GetString() ?? "";
        string orderInfo = root.GetProperty("orderInfo").GetString() ?? "";
        string partnerCode = root.GetProperty("partnerCode").GetString() ?? "";

        var rawSignString = $"accessKey={options.AccessKey}&amount={amount}&extraData={extraData}&message={message}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&requestId={requestId}&responseTime={responseTime}&resultCode={resultCode}";
        
        string expectedSignature = SignHmacSha256(rawSignString, options.SecretKey);
        if (signature != expectedSignature)
        {
            return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "Chữ ký không hợp lệ" };
        }

        // 3. Process Business Logic
        if (resultCode != 0)
        {
            logger.LogWarning("Thanh toán MoMo thất bại cho {OrderId}: {Message}", orderId, message);
            return new MoMoWebhookResult { IsSuccess = true }; // Still return true so MoMo stops retrying
        }

        var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.InvoiceCode == invoiceCode);
        if (invoice == null) return new MoMoWebhookResult { IsSuccess = false, ErrorMessage = "Không tìm thấy hóa đơn" };

        if (invoice.Status != InvoiceStatus.Paid)
        {
            // Record transaction
            var transaction = new Transaction
            {
                InvoiceId = invoice.Id,
                Amount = amount,
                Method = TransactionMethod.Momo,
                Direction = TransactionDirection.Income,
                Category = TransactionCategory.Rent,
                ProviderRef = transId,
                ProviderResponse = rawBody,
                Status = TransactionStatus.Success,
                Note = $"Thanh toán MoMo cho {invoiceCode}",
                PaidAt = DateTime.UtcNow
            };
            context.Transactions.Add(transaction);

            // Update invoice
            invoice.Status = InvoiceStatus.Paid;
            invoice.PaidAt = DateTime.UtcNow;

            await context.SaveChangesAsync(CancellationToken.None);

            // Enqueue confirmation job (to be implemented)
            // backgroundJobService.Enqueue<SendPaymentConfirmationJob>(j => j.ExecuteAsync(invoice.Id, CancellationToken.None));
            
            // Set Redis idempotency
            await db.StringSetAsync(redisKey, "processed", TimeSpan.FromDays(1));
        }

        return new MoMoWebhookResult { IsSuccess = true };
    }

    private string SignHmacSha256(string message, string key)
    {
        byte[] keyByte = Encoding.UTF8.GetBytes(key);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        using var hmacsha256 = new HMACSHA256(keyByte);
        byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
        return BitConverter.ToString(hashmessage).Replace("-", "").ToLower();
    }
}
