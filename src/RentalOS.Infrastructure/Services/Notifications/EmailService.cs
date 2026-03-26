using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace RentalOS.Infrastructure.Services.Notifications;

public class EmailService(IDbConnection dbConnection, ILogger<EmailService> logger) : IEmailService
{
    public async Task<bool> SendEmailAsync(string to, string subject, string body, Guid tenantId)
    {
        logger.LogInformation("Sending Email to {To} for Tenant {TenantId}: {Subject}", to, tenantId, subject);

        // Mock SendGrid API Call
        await Task.Delay(200);
        bool success = true;

        await LogNotification("email", to, subject, success, tenantId);

        return success;
    }

    private async Task LogNotification(string channel, string recipient, string subject, bool success, Guid tenantId)
    {
        const string sql = @"
            INSERT INTO notification_logs (tenant_id, channel, event_type, recipient, status, created_at)
            VALUES (@tenantId, @channel, 'email_alert', @recipient, @status, NOW())";
        
        await dbConnection.ExecuteAsync(sql, new 
        { 
            tenantId, 
            channel, 
            recipient, 
            status = success ? "success" : "failed" 
        });
    }
}
