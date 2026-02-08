namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Payment transaction status
/// </summary>
public enum PaymentStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3,
    Expired = 4,
    Cancelled = 5

    // Refunded = 6,
    // PartiallyRefunded = 7
}