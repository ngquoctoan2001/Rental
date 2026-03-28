using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Queries.GetNotificationLogs;

public record GetNotificationLogsQuery(
    string? Channel = null,
    string? Status = null,
    DateTime? DateFrom = null,
    DateTime? DateTo = null) : IRequest<List<NotificationLogDto>>;

public class NotificationLogDto
{
    public Guid Id { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Recipient { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetNotificationLogsQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetNotificationLogsQuery, List<NotificationLogDto>>
{
    public async Task<List<NotificationLogDto>> Handle(GetNotificationLogsQuery request, CancellationToken cancellationToken)
    {
        if (dbContext.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
            await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();
        var sql = @"
            SELECT id, channel, event_type as EventType, COALESCE(recipient_name, recipient_email, recipient_phone, '') as Recipient, status, error_message as ErrorMessage, created_at as CreatedAt
            FROM notification_logs
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!string.IsNullOrEmpty(request.Channel))
        {
            sql += " AND channel = @channel";
            parameters.Add("channel", request.Channel);
        }

        if (!string.IsNullOrEmpty(request.Status))
        {
            sql += " AND status = @status";
            parameters.Add("status", request.Status);
        }

        sql += " ORDER BY created_at DESC LIMIT 100";

        var logs = await connection.QueryAsync<NotificationLogDto>(sql, parameters);
        return logs.ToList();
    }
}
