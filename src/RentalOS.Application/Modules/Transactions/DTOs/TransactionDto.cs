using RentalOS.Domain.Enums;

namespace RentalOS.Application.Modules.Transactions.DTOs;

public class TransactionDto
{
    public Guid Id { get; set; }
    public Guid? InvoiceId { get; set; }
    public string? InvoiceCode { get; set; }
    public string? TransactionCode { get; set; }
    public decimal Amount { get; set; }
    public TransactionMethod Method { get; set; }
    public TransactionDirection Direction { get; set; }
    public TransactionCategory Category { get; set; }
    public string? ProviderRef { get; set; }
    public TransactionStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Additional info for display
    public string? RoomNumber { get; set; }
    public string? PropertyName { get; set; }
    public string? CustomerName { get; set; }
}

public class TransactionSummaryDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal NetCashflow => TotalIncome - TotalExpense;
    public Dictionary<string, decimal> ByMethod { get; set; } = new();
    public Dictionary<string, decimal> ByCategory { get; set; } = new();
    public decimal CollectionRate { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalCollected { get; set; }
    public decimal TotalOutstanding { get; set; }
}
