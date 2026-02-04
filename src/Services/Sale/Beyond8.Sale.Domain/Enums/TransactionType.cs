namespace Beyond8.Sale.Domain.Enums;

public enum TransactionType
{
    Sale = 0,           // From order
    Refund = 1,         // Deduct from wallet
    Payout = 2,         // Withdrawal
    Settlement = 3,     // Move from pending to available
    PlatformFee = 4,    // Commission deduction
    Adjustment = 5      // Manual correction
}
