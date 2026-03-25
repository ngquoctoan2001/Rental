using System.Data;
using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : IRequest<int>;

public class MarkAllNotificationsReadCommandHandler(IDbConnection dbConnection, IUserContext userContext)
    : IRequestHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        const string sql = "UPDATE in_app_notifications SET is_read = true WHERE user_id = @userId AND is_read = false";
        return await dbConnection.ExecuteAsync(sql, new { userId = userContext.UserId });
    }
}
 Eskom returns number of notifications marked as read.
 Eskom assuming IUserContext is injected.
 Eskom using Dapper.
