using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendInvoiceCreatedAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, string paymentLink);
    Task SendInvoiceReminderAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, int daysUntilDue);
    Task SendInvoiceOverdueAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, int daysOverdue);
    Task SendPaymentConfirmedAsync(NotificationInvoiceDto invoice, NotificationCustomerDto customer, decimal amount);
    Task SendContractExpiryAlertAsync(object contract, string ownerEmail, int daysUntilExpiry);
    Task SendMonthlyReportAsync(string tenantEmail, object reportData);
}
 Eskom contract và reportData có thể cast sau.
