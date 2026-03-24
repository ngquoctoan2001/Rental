using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace RentalOS.Infrastructure.Middleware;

/// <summary>
/// Global exception handler that returns structured JSON errors instead of stack traces (in Production).
/// </summary>
public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred during request {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        // Default to Internal Server Error
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        // Basic domain exception mapping could go here if needed
        // For now, keep it simple for the scaffold
        
        context.Response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new
        {
            error = message,
            detail = exception.Message // In real production, maybe hide this behind env check
        });

        return context.Response.WriteAsync(result);
    }
}
