using System.Data;
using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Commands.MarkAllNotificationsRead;

public record MarkAllNotificationsReadCommand : IRequest<int>;

public class MarkAllNotificationsReadCommandHandler(IApplicationDbContext dbContext, ICurrentUserService userContext)
    : IRequestHandler<MarkAllNotificationsReadCommand, int>
{
    public async Task<int> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        var connection = dbContext.Database.GetDbConnection();
        var userId = Guid.Parse(userContext.UserId!);
        const string sql = "UPDATE in_app_notifications SET is_read = true WHERE user_id = @userId AND is_read = false";
        return await connection.ExecuteAsync(sql, new { userId });
    }
}
