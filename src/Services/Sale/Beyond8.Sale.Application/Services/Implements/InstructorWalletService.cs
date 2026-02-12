using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Application.Mappings.Wallets;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class InstructorWalletService(
    ILogger<InstructorWalletService> logger,
    IUnitOfWork unitOfWork) : IInstructorWalletService
{
    public async Task<ApiResponse<InstructorWalletResponse>> GetWalletByInstructorAsync(Guid instructorId)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<InstructorWalletResponse>.FailureResponse("Không tìm thấy ví giảng viên");

        return ApiResponse<InstructorWalletResponse>.SuccessResponse(
            wallet.ToResponse(), "Lấy thông tin ví thành công");
    }

    public async Task<ApiResponse<List<WalletTransactionResponse>>> GetWalletTransactionsAsync(
        Guid instructorId, PaginationRequest pagination)
    {
        var wallet = await unitOfWork.InstructorWalletRepository
            .FindOneAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<List<WalletTransactionResponse>>.FailureResponse("Không tìm thấy ví giảng viên");

        var transactions = await unitOfWork.TransactionLedgerRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: t => t.WalletId == wallet.Id,
            orderBy: q => q.OrderByDescending(t => t.CreatedAt));

        return ApiResponse<List<WalletTransactionResponse>>.SuccessPagedResponse(
            transactions.Items.Select(t => t.ToTransactionResponse()).ToList(),
            transactions.TotalCount,
            pagination.PageNumber,
            pagination.PageSize,
            "Lấy lịch sử giao dịch thành công");
    }

    /// <summary>
    /// Credit instructor earnings immediately after payment success (Phase 2 — no escrow)
    /// Per BR-19: 70% Instructor / 30% Platform
    /// </summary>
    public async Task<ApiResponse<bool>> CreditEarningsAsync(
        Guid instructorId, decimal amount, Guid orderId, string description)
    {
        var wallet = await GetOrCreateWalletAsync(instructorId);

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance += amount;
        wallet.TotalEarnings += amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Create audit trail transaction
        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.Sale,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = orderId,
            ReferenceType = "Order",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Earnings credited — InstructorId: {InstructorId}, Amount: {Amount}, OrderId: {OrderId}, BalanceAfter: {BalanceAfter}",
            instructorId, amount, orderId, wallet.AvailableBalance);

        return ApiResponse<bool>.SuccessResponse(true, "Đã cộng tiền vào ví giảng viên");
    }

    /// <summary>
    /// Credit wallet from VNPay top-up
    /// </summary>
    public async Task<ApiResponse<bool>> CreditTopUpAsync(
        Guid instructorId, decimal amount, Guid paymentId, string description)
    {
        var wallet = await GetOrCreateWalletAsync(instructorId);

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.TopUp,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = paymentId,
            ReferenceType = "Payment",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "TopUp credited — InstructorId: {InstructorId}, Amount: {Amount}, PaymentId: {PaymentId}, BalanceAfter: {BalanceAfter}",
            instructorId, amount, paymentId, wallet.AvailableBalance);

        return ApiResponse<bool>.SuccessResponse(true, "Nạp tiền vào ví thành công");
    }

    /// <summary>
    /// Deduct funds from wallet for payout processing
    /// </summary>
    public async Task<ApiResponse<bool>> DeductForPayoutAsync(
        Guid instructorId, decimal amount, Guid payoutId, string description)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy ví giảng viên");

        if (wallet.AvailableBalance < amount)
            return ApiResponse<bool>.FailureResponse("Số dư không đủ để thực hiện rút tiền");

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance -= amount;
        wallet.TotalWithdrawn += amount;
        wallet.LastPayoutAt = DateTime.UtcNow;
        wallet.UpdatedAt = DateTime.UtcNow;

        // Create audit trail transaction
        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.Payout,
            Status = TransactionStatus.Completed,
            Amount = amount,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = payoutId,
            ReferenceType = "Payout",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Payout deducted — InstructorId: {InstructorId}, Amount: {Amount}, PayoutId: {PayoutId}, BalanceAfter: {BalanceAfter}",
            instructorId, amount, payoutId, wallet.AvailableBalance);

        return ApiResponse<bool>.SuccessResponse(true, "Đã trừ tiền từ ví giảng viên");
    }

    /// <summary>
    /// Create wallet when instructor is approved (consumed from Identity InstructorApprovalEvent)
    /// </summary>
    public async Task<ApiResponse<InstructorWalletResponse>> CreateWalletAsync(Guid instructorId)
    {
        var existingWallet = await unitOfWork.InstructorWalletRepository
            .FindOneAsync(w => w.InstructorId == instructorId);

        if (existingWallet != null)
            return ApiResponse<InstructorWalletResponse>.SuccessResponse(
                existingWallet.ToResponse(), "Ví giảng viên đã tồn tại");

        var wallet = new InstructorWallet
        {
            InstructorId = instructorId,
            AvailableBalance = 0,
            TotalEarnings = 0,
            TotalWithdrawn = 0,
            Currency = "VND",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.InstructorWalletRepository.AddAsync(wallet);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Wallet created for instructor {InstructorId}", instructorId);

        return ApiResponse<InstructorWalletResponse>.SuccessResponse(
            wallet.ToResponse(), "Tạo ví giảng viên thành công");
    }

    // ── Coupon Hold/Release Methods ──

    /// <summary>
    /// Hold funds when instructor creates coupon.
    /// Moves from AvailableBalance → HoldBalance.
    /// </summary>
    public async Task<ApiResponse<bool>> HoldFundsForCouponAsync(
        Guid instructorId, decimal holdAmount, Guid couponId, string description)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy ví giảng viên");

        if (wallet.AvailableBalance < holdAmount)
            return ApiResponse<bool>.FailureResponse(
                $"Số dư không đủ để tạo coupon. Cần tối thiểu {holdAmount:N0} VND, hiện có {wallet.AvailableBalance:N0} VND");

        var balanceBefore = wallet.AvailableBalance;
        wallet.AvailableBalance -= holdAmount;
        wallet.HoldBalance += holdAmount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.CouponHold,
            Status = TransactionStatus.Completed,
            Amount = holdAmount,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = couponId,
            ReferenceType = "Coupon",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Coupon hold placed — InstructorId: {InstructorId}, HoldAmount: {HoldAmount}, CouponId: {CouponId}, AvailableAfter: {Available}, HoldAfter: {Hold}",
            instructorId, holdAmount, couponId, wallet.AvailableBalance, wallet.HoldBalance);

        return ApiResponse<bool>.SuccessResponse(true, "Đã giữ tiền cho coupon");
    }

    /// <summary>
    /// Deduct actual discount from HoldBalance when coupon is used on a paid order.
    /// The instructor absorbs this discount cost.
    /// </summary>
    public async Task<ApiResponse<bool>> DeductCouponUsageFromHoldAsync(
        Guid instructorId, decimal actualDiscount, Guid couponId, Guid orderId, string description)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId && w.DeletedAt == null);

        if (wallet == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy ví giảng viên");

        // Deduct from hold (should always have enough due to initial hold)
        var deductAmount = Math.Min(actualDiscount, wallet.HoldBalance);
        wallet.HoldBalance -= deductAmount;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.CouponUsage,
            Status = TransactionStatus.Completed,
            Amount = deductAmount,
            Currency = "VND",
            BalanceBefore = wallet.AvailableBalance + deductAmount + wallet.HoldBalance, // Pre-deduct total
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = orderId,
            ReferenceType = "Order",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        // Don't SaveChanges here — caller (PaymentService) will SaveChanges after all wallet operations

        logger.LogInformation(
            "Coupon usage deducted from hold — InstructorId: {InstructorId}, Amount: {Amount}, CouponId: {CouponId}, OrderId: {OrderId}",
            instructorId, deductAmount, couponId, orderId);

        return ApiResponse<bool>.SuccessResponse(true, "Đã trừ tiền coupon từ số dư giữ");
    }

    /// <summary>
    /// Release remaining hold back to AvailableBalance when coupon is deactivated/deleted/fully used.
    /// </summary>
    public async Task<ApiResponse<bool>> ReleaseCouponHoldAsync(
        Guid instructorId, decimal releaseAmount, Guid couponId, string description)
    {
        if (releaseAmount <= 0)
            return ApiResponse<bool>.SuccessResponse(true, "Không có số dư giữ cần hoàn trả");

        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet == null)
            return ApiResponse<bool>.FailureResponse("Không tìm thấy ví giảng viên");

        var releaseActual = Math.Min(releaseAmount, wallet.HoldBalance);
        var balanceBefore = wallet.AvailableBalance;
        wallet.HoldBalance -= releaseActual;
        wallet.AvailableBalance += releaseActual;
        wallet.UpdatedAt = DateTime.UtcNow;

        var transaction = new TransactionLedger
        {
            WalletId = wallet.Id,
            Type = TransactionType.CouponRelease,
            Status = TransactionStatus.Completed,
            Amount = releaseActual,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = wallet.AvailableBalance,
            ReferenceId = couponId,
            ReferenceType = "Coupon",
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.TransactionLedgerRepository.AddAsync(transaction);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Coupon hold released — InstructorId: {InstructorId}, Released: {Amount}, CouponId: {CouponId}, AvailableAfter: {Available}",
            instructorId, releaseActual, couponId, wallet.AvailableBalance);

        return ApiResponse<bool>.SuccessResponse(true, "Đã hoàn trả số dư giữ từ coupon");
    }

    // ── Private Helpers ──

    /// <summary>
    /// Get existing wallet or auto-create if not found (defensive for CreditEarnings)
    /// </summary>
    private async Task<InstructorWallet> GetOrCreateWalletAsync(Guid instructorId)
    {
        var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
            .FirstOrDefaultAsync(w => w.InstructorId == instructorId);

        if (wallet != null)
            return wallet;

        // Auto-create wallet if not found (defensive — normally created via InstructorApprovalEvent)
        wallet = new InstructorWallet
        {
            InstructorId = instructorId,
            AvailableBalance = 0,
            TotalEarnings = 0,
            TotalWithdrawn = 0,
            Currency = "VND",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await unitOfWork.InstructorWalletRepository.AddAsync(wallet);
        // Don't SaveChanges here — caller will SaveChanges after updating balance

        logger.LogWarning("Auto-created wallet for instructor {InstructorId} during payment processing", instructorId);

        return wallet;
    }
}
