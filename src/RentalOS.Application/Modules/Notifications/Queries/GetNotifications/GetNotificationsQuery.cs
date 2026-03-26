using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Queries.GetNotifications;

public record GetNotificationsQuery : IRequest<NotificationListDto>;

public class NotificationListDto
{
    public List<NotificationItemDto> Items { get; set; } = [];
    public int UnreadCount { get; set; }
}

public class NotificationItemDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetNotificationsQueryHandler(IApplicationDbContext dbContext, ICurrentUserService userContext)
    : IRequestHandler<GetNotificationsQuery, NotificationListDto>
{
    public async Task<NotificationListDto> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        var userId = userContext.UserId;
        const string sql = @"
            SELECT id, type, title, message, is_read as IsRead, created_at as CreatedAt
            FROM in_app_notifications
            WHERE user_id = @userId
            ORDER BY created_at DESC LIMIT 50;

            SELECT COUNT(*) FROM in_app_notifications WHERE user_id = @userId AND is_read = false;";

        using var multi = await connection.QueryMultipleAsync(sql, new { userId });
        var items = (await multi.ReadAsync<NotificationItemDto>()).ToList();
        var unreadCount = await multi.ReadFirstAsync<int>();

        return new NotificationListDto
        {
            Items = items,
            UnreadCount = unreadCount
        };
    }
}
