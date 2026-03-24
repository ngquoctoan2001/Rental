namespace RentalOS.API.Middleware;

public class RateLimitMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: implement Redis-backed rate limiting
        await next(context);
    }
}
