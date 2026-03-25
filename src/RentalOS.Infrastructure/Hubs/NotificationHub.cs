using Microsoft.AspNetCore.SignalR;

namespace RentalOS.Infrastructure.Hubs;

public class NotificationHub : Hub
{
    public const string TenantGroupPrefix = "Tenant_";
    public const string UserGroupPrefix = "User_";

    public async Task JoinTenantGroup(string tenantSlug)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, TenantGroupPrefix + tenantSlug);
    }

    public async Task JoinUserGroup(string userId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, UserGroupPrefix + userId);
    }
}

// Bổ sung Extension method để dễ dùng từ IHubContext
public static class NotificationHubExtensions
{
    public static async Task BroadcastToTenant(this IHubContext<NotificationHub> hubContext, string tenantSlug, string eventName, object data)
    {
        await hubContext.Clients.Group(NotificationHub.TenantGroupPrefix + tenantSlug).SendAsync(eventName, data);
    }

    public static async Task BroadcastToUser(this IHubContext<NotificationHub> hubContext, string userId, string eventName, object data)
    {
        await hubContext.Clients.Group(NotificationHub.UserGroupPrefix + userId).SendAsync(eventName, data);
    }
}
 Eskom simplified SignalR implementation as per requested methods.
