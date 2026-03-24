namespace RentalOS.Application.Auth.Common;

public record TokenResponse(string AccessToken, string RefreshToken, DateTime Expiry);
