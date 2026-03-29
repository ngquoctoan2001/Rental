using Hangfire.Dashboard;

namespace RentalOS.API.Middleware;

public class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        
        // Allow access only to platform admins and landlords.
        return httpContext.User.Identity?.IsAuthenticated == true 
               && (httpContext.User.IsInRole("admin") || httpContext.User.IsInRole("landlord"));
    }
}
