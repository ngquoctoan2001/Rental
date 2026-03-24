using RentalOS.Domain.Enums;
using System.Text.Json.Nodes;

namespace RentalOS.Domain.Entities;

/// <summary>A financial transaction (income or expense) — bảng 10.</summary>
public class Transaction : BaseEntity
{
    /// <summary>FK to the invoice this transaction settles (nullable for non-invoice payments).</summary>
    public Guid? InvoiceId { get; set; }

    /// <summary>Provider-agnostic transaction reference code.</summary>
    public string? TransactionCode { get; set; }

    /// <summary>Amount of money transferred (VND).</summary>
    public decimal Amount { get; set; }

    /// <summary>Payment method used.</summary>
    public TransactionMethod Method { get; set; }

    /// <summary>Whether money flows into or out of the owner's account.</summary>
    public TransactionDirection Direction { get; set; }

    /// <summary>Business category of the transaction.</summary>
    public TransactionCategory Category { get; set; } = TransactionCategory.Rent;

    /// <summary>Reference ID returned by the payment provider (Momo/VNPay order ID).</summary>
    public string? ProviderRef { get; set; }

    /// <summary>Raw JSON response from the payment provider.</summary>
    public string? ProviderResponse { get; set; }

    /// <summary>Execution status.</summary>
    public TransactionStatus Status { get; set; } = TransactionStatus.Success;

    /// <summary>Free-text note about this transaction.</summary>
    public string? Note { get; set; }

    /// <summary>URL of the receipt image (e.g. bank transfer screenshot). MỚI.</summary>
    public string? ReceiptUrl { get; set; }

    /// <summary>FK to the user who recorded this transaction.</summary>
    public Guid? RecordedBy { get; set; }

    /// <summary>Actual payment datetime (may differ from CreatedAt for offline payments).</summary>
    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Invoice? Invoice { get; set; }
}
