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

    /// <summary>
    /// Credit platform revenue (30% commission) after payment success.
    /// Platform balance increases.
    /// </summary>
    public async Task CreditPlatformRevenueAsync(decimal platformFee, Guid orderId, string description)
    {
        var wallet = await GetOrCreatePlatformWalletAsync();

        wallet.AvailableBalance += platformFee;
        wallet.TotalRevenue += platformFee;
        wallet.UpdatedAt = DateTime.UtcNow;

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

        wallet.AvailableBalance -= discountAmount;
        wallet.TotalCouponCost += discountAmount;
        wallet.UpdatedAt = DateTime.UtcNow;

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
