using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Application.Mappings.Wallets;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class PlatformWalletService(
    ILogger<PlatformWalletService> logger,
    IUnitOfWork unitOfWork) : IPlatformWalletService
{
    public async Task<ApiResponse<PlatformWalletResponse>> GetPlatformWalletAsync()
    {
        var wallet = await GetOrCreatePlatformWalletAsync();

        return ApiResponse<PlatformWalletResponse>.SuccessResponse(
            wallet.ToResponse(), "Lấy thông tin ví nền tảng thành công");
    }

    public async Task<ApiResponse<List<PlatformWalletTransactionResponse>>> GetPlatformWalletTransactionsAsync(PaginationRequest pagination)
    {
        var wallet = await GetOrCreatePlatformWalletAsync();

        var transactions = await unitOfWork.PlatformWalletTransactionRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: t => t.PlatformWalletId == wallet.Id,
            orderBy: query => query.OrderByDescending(t => t.CreatedAt));

        var responses = transactions.Items.Select(t => t.ToResponse()).ToList();

        return ApiResponse<List<PlatformWalletTransactionResponse>>.SuccessPagedResponse(
            responses, transactions.TotalCount, pagination.PageNumber, pagination.PageSize,
            "Lấy lịch sử giao dịch ví nền tảng thành công");
    }

    /// <summary>
    /// Credit platform revenue (30% commission) after payment success.
    /// Platform balance increases.
    /// </summary>
    public async Task CreditPlatformRevenueAsync(decimal platformFee, Guid orderId, string description)
    {
        var wallet = await GetOrCreatePlatformWalletAsync();

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance += platformFee;
        wallet.TotalRevenue += platformFee;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Create transaction record
        var transaction = new PlatformWalletTransaction
        {
            PlatformWalletId = wallet.Id,
            ReferenceId = orderId,
            ReferenceType = "Order",
            Type = Domain.Enums.PlatformTransactionType.Revenue,
            Amount = platformFee,
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PlatformWalletTransactionRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Platform revenue credited — Amount: {Amount}, OrderId: {OrderId}, BalanceAfter: {Balance}",
            platformFee, orderId, wallet.AvailableBalance);
    }

    /// <summary>
    /// Debit platform wallet for system coupon cost.
    /// Platform absorbs discount — balance CAN go negative.
    /// Will auto-offset via 30% commission revenue.
    /// </summary>
    public async Task DebitSystemCouponCostAsync(decimal discountAmount, Guid orderId, string description)
    {
        var wallet = await GetOrCreatePlatformWalletAsync();

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance -= discountAmount;
        wallet.TotalCouponCost += discountAmount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Create transaction record
        var transaction = new PlatformWalletTransaction
        {
            PlatformWalletId = wallet.Id,
            ReferenceId = orderId,
            ReferenceType = "Order",
            Type = Domain.Enums.PlatformTransactionType.CouponCost,
            Amount = -discountAmount, // Negative for debit
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PlatformWalletTransactionRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Platform coupon cost debited — Amount: {Amount}, OrderId: {OrderId}, BalanceAfter: {Balance} (can be negative)",
            discountAmount, orderId, wallet.AvailableBalance);
    }

    // ── Private Helpers ──

    /// <summary>
    /// Get existing platform wallet or auto-create one (singleton pattern).
    /// Uses tracked entity for balance updates.
    /// </summary>
    private async Task<PlatformWallet> GetOrCreatePlatformWalletAsync()
    {
        var wallet = await unitOfWork.PlatformWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.DeletedAt == null);

        if (wallet != null)
            return wallet;

        wallet = new PlatformWallet
        {
            AvailableBalance = 0,
            TotalRevenue = 0,
            TotalCouponCost = 0,
            Currency = "VND",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.PlatformWalletRepository.AddAsync(wallet);
        // Don't SaveChanges here — caller will SaveChanges after updating balance

        logger.LogInformation("Platform wallet auto-created");

        return wallet;
    }
}
