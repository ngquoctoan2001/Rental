using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Dtos;

public record ContractCoTenantDto
{
    public Guid Id { get; init; }
    public Guid CustomerId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public record ContractDto
{
    public Guid Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public int RoomFloor { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public string CustomerPhone { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public decimal DepositAmount { get; init; }
    public bool DepositPaid { get; init; }
    public bool SignedByCustomer { get; init; }
    public ContractStatus Status { get; init; }
    public string? PdfUrl { get; init; }
    public List<ContractCoTenantDto> CoTenants { get; init; } = [];
}
