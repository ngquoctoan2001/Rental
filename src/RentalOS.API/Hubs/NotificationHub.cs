using Microsoft.AspNetCore.SignalR;

namespace RentalOS.API.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        // TODO: join tenant-specific group
        await base.OnConnectedAsync();
    }
}
