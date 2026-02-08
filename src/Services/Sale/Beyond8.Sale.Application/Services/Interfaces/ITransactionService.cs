using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Transactions;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface ITransactionService
{
    // ── Internal (called by PaymentService / PayoutService) ──
    Task<ApiResponse<TransactionLedgerResponse>> CreateTransactionAsync(CreateTransactionRequest request);

    // ── Instructor / Admin ──
    Task<ApiResponse<TransactionLedgerResponse>> GetTransactionByIdAsync(Guid transactionId);
    Task<ApiResponse<List<TransactionLedgerResponse>>> GetTransactionsByWalletAsync(Guid walletId, PaginationRequest pagination);

    // ── Admin Only ──
    Task<ApiResponse<List<TransactionLedgerResponse>>> GetAllTransactionsAsync(PaginationRequest pagination);
    Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
}