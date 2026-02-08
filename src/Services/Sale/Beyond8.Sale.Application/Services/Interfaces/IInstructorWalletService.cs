using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IInstructorWalletService
{
    Task<ApiResponse<InstructorWalletResponse>> GetWalletByInstructorAsync(Guid instructorId);

    /// <summary>
    /// Credit instructor earnings immediately after payment success (no escrow)
    /// </summary>
    Task<ApiResponse<bool>> CreditEarningsAsync(Guid instructorId, decimal amount, Guid orderId, string description);

    /// <summary>
    /// Deduct funds for payout processing
    /// </summary>
    Task<ApiResponse<bool>> DeductForPayoutAsync(Guid instructorId, decimal amount, Guid payoutId, string description);

    Task<ApiResponse<List<WalletTransactionResponse>>> GetWalletTransactionsAsync(Guid instructorId, PaginationRequest pagination);

    /// <summary>
    /// Create wallet when instructor is approved (consumed from Identity event)
    /// </summary>
    Task<ApiResponse<InstructorWalletResponse>> CreateWalletAsync(Guid instructorId);
}