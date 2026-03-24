using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace RentalOS.API.Hubs;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantSlug = Context.User?.FindFirst("tenant_slug")?.Value;
        if (!string.IsNullOrEmpty(tenantSlug))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, tenantSlug);
        }

        await base.OnConnectedAsync();
    }

    public async Task SendNotification(string userId, object notification)
    {
        // SignalR standard implementation to send to a specific user
        await Clients.User(userId).SendAsync("ReceiveNotification", notification);
    }
}
