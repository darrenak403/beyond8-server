using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Transactions;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface ITransactionService
{
    Task<ApiResponse<TransactionLedgerResponse>> CreateTransactionAsync(CreateTransactionRequest request);
    Task<ApiResponse<TransactionLedgerResponse>> GetTransactionByIdAsync(Guid transactionId);
    Task<ApiResponse<List<TransactionLedgerResponse>>> GetTransactionsByUserAsync(Guid userId, PaginationRequest pagination);
    Task<ApiResponse<List<TransactionLedgerResponse>>> GetAllTransactionsAsync(PaginationRequest pagination);
    Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
}