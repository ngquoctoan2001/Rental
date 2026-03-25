using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace RentalOS.Infrastructure.Services.Notifications;

public class SmsService(IDbConnection dbConnection, ILogger<SmsService> logger) : ISmsService
{
    public async Task<bool> SendSmsAsync(string phone, string message, Guid tenantId)
    {
        logger.LogInformation("Sending SMS to {Phone} for Tenant {TenantId}: {Message}", phone, tenantId, message);

        // Mock VNPT SMS API Call
        await Task.Delay(50);
        bool success = true;

        await LogNotification("sms", phone, message, success, tenantId);

        return success;
    }

    private async Task LogNotification(string channel, string recipient, string message, bool success, Guid tenantId)
    {
        const string sql = @"
            INSERT INTO notification_logs (tenant_id, channel, event_type, recipient, status, created_at)
            VALUES (@tenantId, @channel, 'direct_message', @recipient, @status, NOW())";
        
        await dbConnection.ExecuteAsync(sql, new 
        { 
            tenantId, 
            channel, 
            recipient, 
            status = success ? "success" : "failed" 
        });
    }
}
