using System.Data;
using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Notifications.Commands.MarkNotificationRead;

public record MarkNotificationReadCommand(Guid Id) : IRequest<bool>;

public class MarkNotificationReadCommandHandler(IDbConnection dbConnection, IUserContext userContext)
    : IRequestHandler<MarkNotificationReadCommand, bool>
{
    public async Task<bool> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE in_app_notifications 
            SET is_read = true 
            WHERE id = @id AND user_id = @userId";
        
        var affected = await dbConnection.ExecuteAsync(sql, new { id = request.Id, userId = userContext.UserId });
        return affected > 0;
    }
}
