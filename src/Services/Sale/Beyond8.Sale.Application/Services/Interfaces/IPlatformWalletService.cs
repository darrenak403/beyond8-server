using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IPlatformWalletService
{
    /// <summary>
    /// Get or create the singleton platform wallet
    /// </summary>
    Task<ApiResponse<PlatformWalletResponse>> GetPlatformWalletAsync();

    /// <summary>
    /// Get platform wallet transactions with pagination
    /// </summary>
    Task<ApiResponse<List<PlatformWalletTransactionResponse>>> GetPlatformWalletTransactionsAsync(PaginationRequest pagination);

    /// <summary>
    /// Credit platform revenue (30% commission) after payment success
    /// </summary>
    Task CreditPlatformRevenueAsync(decimal platformFee, Guid orderId, string description);

    /// <summary>
    /// Debit platform wallet for system coupon cost
    /// </summary>
    Task DebitSystemCouponCostAsync(decimal discountAmount, Guid orderId, string description);
}
