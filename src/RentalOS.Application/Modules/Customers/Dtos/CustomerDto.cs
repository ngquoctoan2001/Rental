using RentalOS.Domain.Entities;

namespace RentalOS.Application.Modules.Customers.Dtos;

public record CustomerDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? IdCardNumber { get; init; }
    public string? IdCardImageFront { get; init; }
    public string? IdCardImageBack { get; init; }
    public string? PortraitImage { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public string? Gender { get; init; }
    public string? Hometown { get; init; }
    public string? CurrentAddress { get; init; }
    public string? Occupation { get; init; }
    public string? Workplace { get; init; }
    public string? EmergencyContactName { get; init; }
    public string? EmergencyContactPhone { get; init; }
    public string? EmergencyContactRelationship { get; init; }
    public bool IsBlacklisted { get; init; }
    public string? BlacklistReason { get; init; }
    public DateTime? BlacklistedAt { get; init; }
    public Guid? BlacklistedBy { get; init; }
    public string? Notes { get; init; }
    public ActiveContractDto? ActiveContract { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record ActiveContractDto
{
    public string ContractCode { get; init; } = string.Empty;
    public string RoomNumber { get; init; } = string.Empty;
    public string PropertyName { get; init; } = string.Empty;
}

public record CustomerListItemDto
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public string? IdCardNumber { get; init; }
    public bool IsBlacklisted { get; init; }
    public ActiveContractDto? ActiveContract { get; init; }
    public DateTime CreatedAt { get; init; }
}
