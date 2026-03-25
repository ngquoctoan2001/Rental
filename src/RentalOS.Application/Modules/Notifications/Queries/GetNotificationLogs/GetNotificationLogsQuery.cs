using System.Data;
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

public class GetNotificationLogsQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    : IRequestHandler<GetNotificationLogsQuery, List<NotificationLogDto>>
{
    public async Task<List<NotificationLogDto>> Handle(GetNotificationLogsQuery request, CancellationToken cancellationToken)
    {
        var sql = @"
            SELECT id, channel, event_type as EventType, recipient, status, error_message as ErrorMessage, created_at as CreatedAt
            FROM notification_logs
            WHERE tenant_id = @tenantId";

        var parameters = new DynamicParameters();
        parameters.Add("tenantId", tenantContext.TenantId);

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

        var logs = await dbConnection.QueryAsync<NotificationLogDto>(sql, parameters);
        return logs.ToList();
    }
}
