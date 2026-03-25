using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Enums;

namespace RentalOS.Infrastructure.Jobs;

/// <summary>
/// Background job to reconcile pending payments that didn't receive a webhook.
/// </summary>
public class PaymentConfirmationJob(
    IApplicationDbContext context,
    ILogger<PaymentConfirmationJob> logger)
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("Starting PaymentConfirmationJob at {Time}", DateTime.UtcNow);

        // Find invoices that were initiated but not paid, 
        // and check their status with the provider if possible.
        // For now, this is a placeholder for future implementation of 
        // MoMo/VNPay Query APIs.
        
        var pendingInvoices = await context.Invoices
            .Where(i => i.Status == InvoiceStatus.Pending && i.PaymentLinkToken != null)
            .ToListAsync();

        foreach (var invoice in pendingInvoices)
        {
            // TODO: Call IMoMoService.QueryStatusAsync or IVNPayService.QueryStatusAsync
            // for invoices initiated recently but not marked PAID.
        }

        logger.LogInformation("Finished PaymentConfirmationJob at {Time}", DateTime.UtcNow);
    }
}
