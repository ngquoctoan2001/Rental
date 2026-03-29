using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalOS.Application.Modules.Notifications.Commands.MarkAllNotificationsRead;
using RentalOS.Application.Modules.Notifications.Commands.MarkNotificationRead;
using RentalOS.Application.Modules.Notifications.Queries.GetNotificationLogs;
using RentalOS.Application.Modules.Notifications.Queries.GetNotifications;

namespace RentalOS.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController(ISender mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationListDto>> GetNotifications()
    {
        return Ok(await mediator.Send(new GetNotificationsQuery()));
    }

    [HttpPost("{id}/mark-read")]
    public async Task<ActionResult<bool>> MarkRead(Guid id)
    {
        return Ok(await mediator.Send(new MarkNotificationReadCommand(id)));
    }

    [HttpPost("mark-all-read")]
    public async Task<ActionResult<int>> MarkAllRead()
    {
        return Ok(await mediator.Send(new MarkAllNotificationsReadCommand()));
    }

    [HttpGet("logs")]
    [Authorize(Roles = "admin,landlord")]
    public async Task<ActionResult<List<NotificationLogDto>>> GetLogs([FromQuery] GetNotificationLogsQuery query)
    {
        return Ok(await mediator.Send(query));
    }
}
