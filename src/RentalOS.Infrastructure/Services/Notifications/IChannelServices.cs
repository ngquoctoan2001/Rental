namespace RentalOS.Infrastructure.Services.Notifications;

public interface IZaloService
{
    Task<bool> SendMessageAsync(string phone, string message, Guid tenantId);
}

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phone, string message, Guid tenantId);
}

public interface IEmailService
{
    Task<bool> SendEmailAsync(string to, string subject, string body, Guid tenantId);
}
