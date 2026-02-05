namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Status of a transaction in ledger
/// </summary>
public enum TransactionStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Cancelled = 3
}