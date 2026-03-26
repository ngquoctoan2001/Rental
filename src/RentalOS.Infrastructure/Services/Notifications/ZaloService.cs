using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;

namespace RentalOS.Infrastructure.Services.Notifications;

public class ZaloService(IDbConnection dbConnection, ILogger<ZaloService> logger) : IZaloService
{
    public async Task<bool> SendMessageAsync(string phone, string message, Guid tenantId)
    {
        logger.LogInformation("Sending Zalo message to {Phone} for Tenant {TenantId}: {Message}", phone, tenantId, message);

        // Mock Zalo API Call
        await Task.Delay(100); 
        bool success = true; // Simulating success

        await LogNotification("zalo", phone, message, success, tenantId);

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
