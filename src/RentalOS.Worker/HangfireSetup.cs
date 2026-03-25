using Hangfire;
using RentalOS.Worker.Jobs;

namespace RentalOS.Worker;

public static class HangfireSetup
{
    public static void RegisterAllJobs(IRecurringJobManager recurringJob)
    {
        // 1. Invoices
        recurringJob.AddOrUpdate<GenerateMonthlyInvoicesJob>(
            "generate-monthly-invoices", 
            j => j.ExecuteAsync(), 
            "0 7 28 * *"); // Default 28th

        recurringJob.AddOrUpdate<SendInvoiceRemindersJob>(
            "send-invoice-reminders",
            j => j.ExecuteAsync(),
            "0 8 * * *");

        recurringJob.AddOrUpdate<MarkOverdueInvoicesJob>(
            "mark-overdue-invoices",
            j => j.ExecuteAsync(),
            "0 0 * * *");

        recurringJob.AddOrUpdate<SendOverdueRemindersJob>(
            "send-overdue-reminders",
            j => j.ExecuteAsync(),
            "0 9 * * *");

        // 2. Contracts
        recurringJob.AddOrUpdate<CheckContractExpiryJob>(
            "check-contract-expiry",
            j => j.ExecuteAsync(),
            "0 9 * * *");

        // 3. Payments & Reports
        recurringJob.AddOrUpdate<ReconcilePaymentsJob>(
            "reconcile-payments",
            j => j.ExecuteAsync(),
            "0 * * * *"); // Hourly

        recurringJob.AddOrUpdate<SendMonthlyReportJob>(
            "send-monthly-report",
            j => j.ExecuteAsync(),
            "0 6 1 * *");

        // 4. Maintenance
        recurringJob.AddOrUpdate<CleanupExpiredTokensJob>(
            "cleanup-expired-tokens",
            j => j.ExecuteAsync(),
            "0 2 * * *");

        recurringJob.AddOrUpdate<RetryFailedNotificationsJob>(
            "retry-notifications",
            j => j.ExecuteAsync(),
            "*/15 * * * *");

        recurringJob.AddOrUpdate<RefreshZaloTokenJob>(
            "refresh-zalo-token",
            j => j.ExecuteAsync(),
            "0 0 */1 * *");
    }
}
 Eskom centralized Hangfire job registration. Eskom precise cron scheduling. Eskom comprehensive automation suite.
