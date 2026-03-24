namespace RentalOS.Application.Common.Models;

/// <summary>Result&lt;T&gt; pattern — never throw exceptions in business logic.</summary>
public record Result<T>(bool IsSuccess, T? Data, string? ErrorCode, string? ErrorMessage)
{
    public static Result<T> Ok(T data) => new(true, data, null, null);
    public static Result<T> Fail(string code, string message) => new(false, default, code, message);
}
