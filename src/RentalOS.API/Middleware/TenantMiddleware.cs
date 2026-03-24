namespace RentalOS.API.Middleware;

public class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: resolve tenant from subdomain / header and set TenantContext
        await next(context);
    }
}
