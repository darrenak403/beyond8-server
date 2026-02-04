namespace Beyond8.Sale.Domain.Enums;

public enum OrderStatus
{
    Pending = 0,
    Paid = 1,
    Failed = 2,
    Refunded = 3,
    PartiallyRefunded = 4,
    Cancelled = 5
}
