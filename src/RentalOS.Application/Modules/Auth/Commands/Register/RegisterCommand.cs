using MediatR;
using RentalOS.Application.Modules.Auth.DTOs;

namespace RentalOS.Application.Modules.Auth.Commands.Register;

public record RegisterCommand : IRequest<AuthResultDto>
{
    public string OwnerName { get; init; } = string.Empty;
    public string OwnerEmail { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
