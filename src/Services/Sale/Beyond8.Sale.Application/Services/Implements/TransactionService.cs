using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Transactions;
using Beyond8.Sale.Application.Mappings.Transactions;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class TransactionService(
    ILogger<TransactionService> logger,
    IUnitOfWork unitOfWork) : ITransactionService
{
    public async Task<ApiResponse<TransactionLedgerResponse>> CreateTransactionAsync(CreateTransactionRequest request)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.Id == request.WalletId && w.DeletedAt == null);

        if (wallet == null)
            return ApiResponse<TransactionLedgerResponse>.FailureResponse("Không tìm thấy ví giảng viên");

        var balanceBefore = wallet.AvailableBalance;
        var balanceAfter = request.Type switch
        {
            TransactionType.Sale or TransactionType.Adjustment => balanceBefore + request.Amount,
            TransactionType.Payout or TransactionType.PlatformFee => balanceBefore - request.Amount,
            _ => balanceBefore
        };

        var transaction = request.ToEntity(balanceBefore, balanceAfter);

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Transaction created — WalletId: {WalletId}, Type: {Type}, Amount: {Amount}",
            request.WalletId, request.Type, request.Amount);

        return ApiResponse<TransactionLedgerResponse>.SuccessResponse(
            transaction.ToResponse(), "Tạo giao dịch thành công");
    }

    public async Task<ApiResponse<TransactionLedgerResponse>> GetTransactionByIdAsync(Guid transactionId)
    {
        var transaction = await unitOfWork.TransactionLedgerRepository.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transactionId && t.DeletedAt == null);

        if (transaction == null)
            return ApiResponse<TransactionLedgerResponse>.FailureResponse("Không tìm thấy giao dịch");

        return ApiResponse<TransactionLedgerResponse>.SuccessResponse(
            transaction.ToResponse(), "Lấy thông tin giao dịch thành công");
    }

    public async Task<ApiResponse<List<TransactionLedgerResponse>>> GetTransactionsByWalletAsync(
        Guid walletId, PaginationRequest pagination)
    {
        var transactions = await unitOfWork.TransactionLedgerRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: t => t.WalletId == walletId && t.DeletedAt == null,
            orderBy: q => q.OrderByDescending(t => t.CreatedAt));

        return ApiResponse<List<TransactionLedgerResponse>>.SuccessPagedResponse(
            transactions.Items.Select(t => t.ToResponse()).ToList(),
            transactions.TotalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy danh sách giao dịch thành công");
    }

    public async Task<ApiResponse<List<TransactionLedgerResponse>>> GetAllTransactionsAsync(PaginationRequest pagination)
    {
        // Combine instructor ledger transactions and platform wallet transactions
        // so admin `/api/v1/transactions` shows subscription/platform revenue entries as well.
        var instructorTxs = await unitOfWork.TransactionLedgerRepository.AsQueryable()
            .Where(t => t.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync();

        var platformTxs = await unitOfWork.PlatformWalletTransactionRepository.AsQueryable()
            .Where(p => p.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync();

        // Map both kinds to unified response model
        var unified = new List<TransactionLedgerResponse>(instructorTxs.Count + platformTxs.Count);

        unified.AddRange(instructorTxs.Select(t => t.ToResponse()));

        unified.AddRange(platformTxs.Select(p => new TransactionLedgerResponse
        {
            Id = p.Id,
            WalletId = p.PlatformWalletId,
            Type = p.Type.ToString(),
            Status = p.Status.ToString(),
            AvailableAt = p.AvailableAt,
            Amount = p.Amount,
            Currency = p.Currency,
            BalanceBefore = p.BalanceBefore,
            BalanceAfter = p.BalanceAfter,
            ReferenceId = p.ReferenceId,
            ReferenceType = p.ReferenceType,
            Description = p.Description,
            ExternalTransactionId = null,
            CreatedAt = p.CreatedAt
        }));

        // Sort by CreatedAt desc and paginate in-memory to provide consistent paging across both sets
        var ordered = unified.OrderByDescending(u => u.CreatedAt).ToList();
        var total = ordered.Count;
        var items = ordered
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return ApiResponse<List<TransactionLedgerResponse>>.SuccessPagedResponse(
            items,
            total,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy tất cả giao dịch thành công");
    }

    /// <summary>
    /// Get total platform revenue (sum of PlatformFeeAmount from paid order items) within a date range.
    /// Uses OrderItem.PlatformFeeAmount which represents the 30% platform commission per BR-19.
    /// </summary>
    public async Task<ApiResponse<decimal>> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
    {
        // Per BR-19: Platform revenue = sum of PlatformFeeAmount from paid orders
        var totalRevenue = await unitOfWork.OrderItemRepository.AsQueryable()
            .Where(oi => oi.Order!.Status == Domain.Enums.OrderStatus.Paid
                         && oi.Order.PaidAt >= startDate
                         && oi.Order.PaidAt <= endDate
                         && oi.DeletedAt == null)
            .SumAsync(oi => oi.PlatformFeeAmount);

        logger.LogInformation(
            "Total platform revenue calculated — StartDate: {StartDate}, EndDate: {EndDate}, Revenue: {Revenue}",
            startDate, endDate, totalRevenue);

        return ApiResponse<decimal>.SuccessResponse(totalRevenue, "Lấy tổng doanh thu thành công");
    }
}
