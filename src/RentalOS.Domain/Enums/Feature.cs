namespace RentalOS.Domain.Enums;

/// <summary>Premium features gated by subscription plan.</summary>
[Flags]
public enum Feature
{
    None             = 0,
    AiAgent          = 1 << 0,
    ZaloOA           = 1 << 1,
    AdvancedReports  = 1 << 2,
    ExcelExport      = 1 << 3,
    ApiAccess        = 1 << 4
}
