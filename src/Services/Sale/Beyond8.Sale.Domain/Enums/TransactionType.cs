namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Type of transaction in instructor wallet
/// </summary>
public enum TransactionType
{
    Sale = 0,
    Payout = 2,
    Settlement = 3,
    PlatformFee = 4,
    Adjustment = 5
}
