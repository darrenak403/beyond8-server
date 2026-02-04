using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IInstructorWalletService
{
    Task<ApiResponse<InstructorWalletResponse>> GetWalletByInstructorAsync(Guid instructorId);
    Task<ApiResponse<bool>> AddFundsAsync(Guid instructorId, decimal amount, string description);
    Task<ApiResponse<bool>> DeductFundsAsync(Guid instructorId, decimal amount, string description);
    Task<ApiResponse<List<WalletTransactionResponse>>> GetWalletTransactionsAsync(Guid instructorId, PaginationRequest pagination);
}