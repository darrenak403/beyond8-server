namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Type of transaction in instructor wallet
/// </summary>
public enum TransactionType
{
    Sale = 0,         // Instructor earnings from course sale (credited immediately)
    Payout = 1,       // Instructor withdrawal
    PlatformFee = 2,  // Platform commission deducted
    Adjustment = 3    // Manual adjustment by admin
}
