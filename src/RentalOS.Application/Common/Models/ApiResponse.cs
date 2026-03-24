namespace RentalOS.Application.Common.Models;

public record ApiResponse<T>(bool Success, T? Data, string? Message = null, object? Meta = null)
{
    public static ApiResponse<T> Ok(T data, object? meta = null) => new(true, data, null, meta);
    public static ApiResponse<T> Fail(string message) => new(false, default, message);
}
