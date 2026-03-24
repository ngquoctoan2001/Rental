namespace RentalOS.Domain.Enums;

/// <summary>How a contract was terminated.</summary>
public enum TerminationType
{
    /// <summary>Normal end at expiry date.</summary>
    Normal,
    /// <summary>Terminated due to contract breach.</summary>
    Breach,
    /// <summary>Mutual agreement between landlord and tenant.</summary>
    Mutual
}
