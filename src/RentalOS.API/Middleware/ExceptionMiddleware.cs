using System.Net;
using RentalOS.Domain.Exceptions;

namespace RentalOS.API.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var (statusCode, errorCode) = exception switch
        {
            ValidationException => (HttpStatusCode.BadRequest, "VALIDATION_ERROR"),
            NotFoundException => (HttpStatusCode.NotFound, "NOT_FOUND"),
            PlanLimitException => (HttpStatusCode.PaymentRequired, "PLAN_LIMIT_REACHED"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "UNAUTHORIZED"),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR")
        };

        context.Response.StatusCode = (int)statusCode;

        // Log the exception using Serilog (mapped through ILogger)
        if (statusCode == HttpStatusCode.InternalServerError)
        {
            logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);
        }
        else
        {
            logger.LogWarning("Handled exception: {Type} - {Message}", exception.GetType().Name, exception.Message);
        }

        var response = new
        {
            success = false,
            error = new
            {
                code = errorCode,
                message = exception.Message,
                details = exception is ValidationException vex ? vex.Errors : null
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }
}

