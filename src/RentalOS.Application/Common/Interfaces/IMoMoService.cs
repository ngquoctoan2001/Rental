using RentalOS.Domain.Entities;

namespace RentalOS.Application.Common.Interfaces;

public interface IMoMoService
{
    Task<MoMoPaymentResult> CreatePaymentAsync(Invoice invoice, string returnUrl, string notifyUrl);
    Task<MoMoWebhookResult> ProcessWebhookAsync(string rawBody, IDictionary<string, string> headers);
}

public class MoMoPaymentResult
{
    public string PayUrl { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public string Deeplink { get; set; } = string.Empty;
}

public class MoMoWebhookResult
{
    public bool IsSuccess { get; set; }
    public bool AlreadyProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}
