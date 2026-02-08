namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Payout request workflow status
/// Requested -> Approved -> Processing -> Completed
/// Requested -> Rejected
/// </summary>
public enum PayoutStatus
{
    Requested = 0,
    Approved = 1,
    Processing = 2,
    Completed = 3,
    Rejected = 4,
    Failed = 5
}