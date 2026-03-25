using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Contracts.Dtos;

public record ContractDto
{
    public Guid Id { get; init; }
    public string ContractCode { get; init; } = string.Empty;
    public Guid RoomId { get; init; }
    public string RoomNumber { get; init; } = string.Empty;
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public decimal MonthlyRent { get; init; }
    public decimal DepositAmount { get; init; }
    public bool DepositPaid { get; init; }
    public ContractStatus Status { get; init; }
    public string? PdfUrl { get; init; }
}
