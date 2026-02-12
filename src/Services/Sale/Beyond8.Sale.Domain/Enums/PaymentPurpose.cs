namespace Beyond8.Sale.Domain.Enums;

/// <summary>
/// Distinguishes what a Payment record is for
/// </summary>
public enum PaymentPurpose
{
    OrderPayment = 0,  // Normal course purchase
    WalletTopUp = 1    // Instructor wallet top-up
}
