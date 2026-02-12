namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Type of transaction in instructor wallet
/// </summary>
public enum TransactionType
{
    Sale = 0,         // Instructor earnings from course sale (credited immediately)
    Payout = 1,       // Instructor withdrawal
    PlatformFee = 2,  // Platform commission (30%)
    Adjustment = 3,   // Manual adjustment by admin
    TopUp = 4,        // Instructor wallet top-up via VNPay
    CouponHold = 5,   // Funds held for instructor coupon commitment
    CouponRelease = 6,// Funds released from expired/deactivated coupon
    CouponUsage = 7   // Funds consumed when instructor coupon is used
}
