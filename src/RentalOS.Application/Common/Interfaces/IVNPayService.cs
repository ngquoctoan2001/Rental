using RentalOS.Domain.Entities;

namespace RentalOS.Application.Common.Interfaces;

public interface IVNPayService
{
    Task<string> CreatePaymentAsync(Invoice invoice, string returnUrl);
    Task<VNPayWebhookResult> ProcessWebhookAsync(IDictionary<string, string> queryParams);
    Task<VNPayReturnResult> ProcessReturnAsync(IDictionary<string, string> queryParams);
}

public class VNPayWebhookResult
{
    public bool IsSuccess { get; set; }
    public bool AlreadyProcessed { get; set; }
    public string? ErrorMessage { get; set; }
}

public class VNPayReturnResult
{
    public bool IsSuccess { get; set; }
    public string? InvoiceCode { get; set; }
}
