using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Services.Notifications;

public class EmailService(IDbConnection dbConnection, ITenantContext tenantContext, ILogger<EmailService> logger) : IEmailService
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
        var schemaName = tenantContext.SchemaName;
        if (string.IsNullOrEmpty(schemaName)) return;

        if (dbConnection.State == ConnectionState.Closed)
            await ((DbConnection)dbConnection).OpenAsync();

        // Set tenant search_path via raw ADO.NET (not parameterisable — avoid DAP005)
        using (var cmd = dbConnection.CreateCommand())
        {
            cmd.CommandText = $"SET search_path TO \"{schemaName}\", public";
            await ((DbCommand)cmd).ExecuteNonQueryAsync();
        }

        const string sql = @"
            INSERT INTO notification_logs (tenant_id, channel, event_type, recipient, status, created_at)
            VALUES (@tenantId, @channel, 'email_alert', @recipient, @status, NOW())";

#pragma warning disable DAP005
        await dbConnection.ExecuteAsync(sql, new
        {
            tenantId,
            channel,
            recipient,
            status = success ? "success" : "failed"
        });
#pragma warning restore DAP005
    }
}
