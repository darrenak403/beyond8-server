using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IInstructorWalletService
{
    // ── Instructor ──
    Task<ApiResponse<InstructorWalletResponse>> GetWalletByInstructorAsync(Guid instructorId);
    Task<ApiResponse<List<WalletTransactionResponse>>> GetWalletTransactionsAsync(Guid instructorId, PaginationRequest pagination);

    // ── Internal (called by PaymentService / PayoutService / Event consumers) ──

    /// <summary>
    /// Credit instructor earnings immediately after payment success (no escrow)
    /// </summary>
    Task<ApiResponse<bool>> CreditEarningsAsync(Guid instructorId, decimal amount, Guid orderId, string description);

    /// <summary>
    /// Credit wallet from VNPay top-up
    /// </summary>
    Task<ApiResponse<bool>> CreditTopUpAsync(Guid instructorId, decimal amount, Guid paymentId, string description);

    /// <summary>
    /// Deduct funds for payout processing
    /// </summary>
    Task<ApiResponse<bool>> DeductForPayoutAsync(Guid instructorId, decimal amount, Guid payoutId, string description);

    /// <summary>
    /// Create wallet when instructor is approved (consumed from Identity event)
    /// </summary>
    Task<ApiResponse<InstructorWalletResponse>> CreateWalletAsync(Guid instructorId);

    // ── Coupon Hold/Release (called by CouponService) ──

    /// <summary>
    /// Hold funds when instructor creates a coupon.
    /// Moves funds from AvailableBalance to HoldBalance.
    /// </summary>
    Task<ApiResponse<bool>> HoldFundsForCouponAsync(Guid instructorId, decimal holdAmount, Guid couponId, string description);

    /// <summary>
    /// Deduct actual discount from held funds when coupon is used.
    /// Reduces HoldBalance by the actual discount amount.
    /// </summary>
    Task<ApiResponse<bool>> DeductCouponUsageFromHoldAsync(Guid instructorId, decimal actualDiscount, Guid couponId, Guid orderId, string description);

    /// <summary>
    /// Release remaining held funds back to AvailableBalance when coupon is deactivated/deleted/expired.
    /// </summary>
    Task<ApiResponse<bool>> ReleaseCouponHoldAsync(Guid instructorId, decimal releaseAmount, Guid couponId, string description);
}