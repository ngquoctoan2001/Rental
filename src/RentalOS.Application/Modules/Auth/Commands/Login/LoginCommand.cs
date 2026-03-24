using MediatR;
using RentalOS.Application.Modules.Auth.DTOs;

namespace RentalOS.Application.Modules.Auth.Commands.Login;

public record LoginCommand : IRequest<AuthResultDto>
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string TenantSlug { get; init; } = string.Empty;
}
