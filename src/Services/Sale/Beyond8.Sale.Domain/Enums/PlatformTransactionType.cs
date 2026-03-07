namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Type of transaction in platform wallet
/// </summary>
public enum PlatformTransactionType
{
    Revenue = 0,      // Platform commission from course sales (30%)
    CouponCost = 1,   // Platform absorbs system coupon discount
    Adjustment = 2    // Manual adjustment by admin
}