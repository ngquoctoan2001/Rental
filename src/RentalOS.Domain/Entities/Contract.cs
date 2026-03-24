using RentalOS.Domain.Enums;

namespace RentalOS.Domain.Entities;

/// <summary>A rental contract linking a room to a primary customer — bảng 7.</summary>
public class Contract : BaseEntity
{
    /// <summary>Human-readable contract code (e.g. "HD-2024-001").</summary>
    public string ContractCode { get; set; } = string.Empty;

    /// <summary>FK to the rented room.</summary>
    public Guid RoomId { get; set; }

    /// <summary>FK to the primary customer (tenant).</summary>
    public Guid CustomerId { get; set; }

    /// <summary>First day of the lease.</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>Last day of the lease.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>Agreed monthly rent amount (VND).</summary>
    public decimal MonthlyRent { get; set; }

    /// <summary>Number of months used as deposit.</summary>
    public int DepositMonths { get; set; } = 1;

    /// <summary>Total deposit amount (VND).</summary>
    public decimal DepositAmount { get; set; }

    /// <summary>Whether the deposit has been collected. MỚI.</summary>
    public bool DepositPaid { get; set; } = false;

    /// <summary>Timestamp when the deposit was received. MỚI.</summary>
    public DateTime? DepositPaidAt { get; set; }

    /// <summary>Amount refunded from the deposit when terminating. MỚI.</summary>
    public decimal? DepositRefunded { get; set; }

    /// <summary>Contracted electricity price per kWh (overrides room default if set).</summary>
    public decimal? ElectricityPrice { get; set; }

    /// <summary>Contracted water price per m³ (overrides room default if set).</summary>
    public decimal? WaterPrice { get; set; }

    /// <summary>Contracted monthly service fee.</summary>
    public decimal? ServiceFee { get; set; }

    /// <summary>Fixed monthly internet fee. MỚI.</summary>
    public decimal? InternetFee { get; set; }

    /// <summary>Fixed monthly garbage collection fee. MỚI.</summary>
    public decimal? GarbageFee { get; set; }

    /// <summary>Day-of-month on which invoices are generated (default: 5).</summary>
    public int BillingDate { get; set; } = 5;

    /// <summary>Number of days after billing date before an invoice is overdue.</summary>
    public int PaymentDueDays { get; set; } = 10;

    /// <summary>Maximum number of occupants allowed.</summary>
    public int MaxOccupants { get; set; } = 2;

    /// <summary>Additional terms and conditions.</summary>
    public string? Terms { get; set; }

    /// <summary>Contract template identifier. MỚI.</summary>
    public string? TemplateId { get; set; }

    /// <summary>URL of the signed PDF contract in R2 storage.</summary>
    public string? PdfUrl { get; set; }

    /// <summary>Lifecycle status of the contract.</summary>
    public ContractStatus Status { get; set; } = ContractStatus.Active;

    /// <summary>Timestamp when the contract was terminated early.</summary>
    public DateTime? TerminatedAt { get; set; }

    /// <summary>Free-text reason for early termination.</summary>
    public string? TerminationReason { get; set; }

    /// <summary>Type of termination. MỚI.</summary>
    public TerminationType? TerminationType { get; set; }

    /// <summary>Whether the customer has signed the contract.</summary>
    public bool SignedByCustomer { get; set; } = false;

    /// <summary>Timestamp when the customer signed.</summary>
    public DateTime? SignedAt { get; set; }

    /// <summary>FK to the user who created the contract.</summary>
    public Guid? CreatedBy { get; set; }

    // Navigation
    public Room Room { get; set; } = null!;
    public Customer Customer { get; set; } = null!;
    public ICollection<ContractCoTenant> CoTenants { get; set; } = [];
    public ICollection<Invoice> Invoices { get; set; } = [];
}
