using Dapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid Id) : IRequest<bool>;

public class MarkNotificationReadCommandHandler(IApplicationDbContext dbContext, ICurrentUserService userContext)
    : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE in_app_notifications 
            SET is_read = true 
            WHERE id = @id AND user_id = @userId";
        var connection = dbContext.Database.GetDbConnection();
        var affected = await connection.ExecuteAsync(sql, new { id = request.Id, userId = userContext.UserId });
        return affected > 0;
    }
}
