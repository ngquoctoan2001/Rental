using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Mail;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Infrastructure.Services.Notifications;

public class EmailService(
    IDbConnection dbConnection,
    ITenantContext tenantContext,
    IConfiguration configuration,
    ILogger<EmailService> logger) : IEmailService
{
    public async Task<bool> SendEmailAsync(string to, string subject, string body, Guid tenantId)
    {
        logger.LogInformation("Sending Email to {To} for Tenant {TenantId}: {Subject}", to, tenantId, subject);

        bool success = false;
        try
        {
            var smtpSection = configuration.GetSection("Smtp");
            string host = smtpSection["Host"] ?? "smtp.gmail.com";
            int port = int.Parse(smtpSection["Port"] ?? "587");
            bool enableSsl = bool.Parse(smtpSection["EnableSsl"] ?? "true");
            string username = smtpSection["Username"] ?? "";
            string password = smtpSection["Password"] ?? "";
            string fromName = smtpSection["FromName"] ?? "RentalOS";
            string fromEmail = smtpSection["FromEmail"] ?? username;

            // Skip actual sending if credentials are not configured
            if (string.IsNullOrWhiteSpace(username) || username.StartsWith("your-"))
            {
                logger.LogWarning("SMTP not configured. Email to {To} skipped.", to);
                success = false;
            }
            else
            {
                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = body.TrimStart().StartsWith("<")
                };
                mail.To.Add(to);

                await client.SendMailAsync(mail);
                success = true;
                logger.LogInformation("Email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}: {Message}", to, ex.Message);
            success = false;
        }

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
