using Beyond8.Common;
using Beyond8.Common.Events.Sale;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Settlements;

namespace Beyond8.Sale.Application.Services.Implements;

public class SettlementService(
    ILogger<SettlementService> logger,
    IUnitOfWork unitOfWork,
    IInstructorWalletService walletService,
    IPublishEndpoint publishEndpoint) : ISettlementService
{
    public async Task<ApiResponse<bool>> ProcessPendingSettlementsAsync()
    {
        var now = DateTime.UtcNow;

        // Find pending sale transactions whose AvailableAt <= now
        var pendingTxs = await unitOfWork.TransactionLedgerRepository.AsQueryable()
            .Where(t => t.Type == TransactionType.Sale && t.Status == TransactionStatus.Pending && t.AvailableAt != null && t.AvailableAt <= now)
            .OrderBy(t => t.AvailableAt)
            .ToListAsync();

        if (!pendingTxs.Any())
        {
            // still attempt platform txs
            await ProcessPendingPlatformTransactionsAsync();
            return ApiResponse<bool>.SuccessResponse(true, "No pending instructor settlements");
        }

        foreach (var tx in pendingTxs)
        {
            try
            {
                // Use execution-strategy-safe transaction wrapper
                Guid? referenceId = null;
                Guid instructorId = Guid.Empty;
                decimal settledAmount = 0m;

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    // Re-load the transaction within the transaction scope and check status again
                    var trackedTx = await unitOfWork.TransactionLedgerRepository.AsQueryable()
                        .FirstOrDefaultAsync(t => t.Id == tx.Id);

                    if (trackedTx == null || trackedTx.Status != TransactionStatus.Pending)
                        return; // already processed by another worker

                    // Load wallet and order within the same transaction
                    var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
                        .FirstOrDefaultAsync(w => w.Id == trackedTx.WalletId);

                    if (wallet == null)
                    {
                        logger.LogWarning("Settlement skipped - wallet not found for tx {TxId}", trackedTx.Id);
                        return;
                    }

                    // Determine settle amount
                    var settleAmount = Math.Min(trackedTx.Amount, wallet.PendingBalance);
                    if (settleAmount <= 0)
                    {
                        logger.LogError("Settlement amount invalid or pending balance insufficient for tx {TxId}", trackedTx.Id);
                        return;
                    }

                    // Apply balances and create settlement ledger entry
                    var balanceBefore = wallet.AvailableBalance;
                    wallet.PendingBalance -= settleAmount;
                    wallet.AvailableBalance += settleAmount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    var settlementTx = new TransactionLedger
                    {
                        WalletId = wallet.Id,
                        Type = TransactionType.Settlement,
                        Status = TransactionStatus.Completed,
                        Amount = settleAmount,
                        Currency = trackedTx.Currency,
                        BalanceBefore = balanceBefore,
                        BalanceAfter = wallet.AvailableBalance,
                        ReferenceId = trackedTx.ReferenceId,
                        ReferenceType = trackedTx.ReferenceType,
                        Description = $"Settlement for order {trackedTx.ReferenceId}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await unitOfWork.TransactionLedgerRepository.AddAsync(settlementTx);

                    // Mark original pending transaction as completed
                    trackedTx.Status = TransactionStatus.Completed;
                    trackedTx.UpdatedAt = DateTime.UtcNow;

                    // Mark related order as settled if present
                    if (trackedTx.ReferenceId.HasValue)
                    {
                        var order = await unitOfWork.OrderRepository.AsQueryable()
                            .FirstOrDefaultAsync(o => o.Id == trackedTx.ReferenceId.Value);
                        if (order != null)
                        {
                            order.IsSettled = true;
                            order.SettledAt = DateTime.UtcNow;
                            order.UpdatedAt = DateTime.UtcNow;
                        }
                    }

                    // capture info for publishing after commit
                    referenceId = trackedTx.ReferenceId;
                    instructorId = wallet.InstructorId;
                    settledAmount = settleAmount;
                });

                // If executed and we have captured data, publish event
                if (settledAmount > 0)
                {
                    await publishEndpoint.Publish(new SettlementCompletedEvent(
                        referenceId ?? Guid.Empty,
                        instructorId,
                        settledAmount,
                        DateTime.UtcNow));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process settlement for tx {TxId}", tx.Id);
            }
        }

        // After instructor settlements, process platform pending transactions too
        await ProcessPendingPlatformTransactionsAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Processed pending settlements");
    }

    // Process pending platform wallet transactions whose AvailableAt <= now
    private async Task ProcessPendingPlatformTransactionsAsync()
    {
        var now = DateTime.UtcNow;

        var pendingPlatformTxs = await unitOfWork.PlatformWalletTransactionRepository.AsQueryable()
            .Where(t => t.Status == TransactionStatus.Pending && t.AvailableAt != null && t.AvailableAt <= now)
            .OrderBy(t => t.AvailableAt)
            .ToListAsync();

        if (!pendingPlatformTxs.Any())
            return;

        foreach (var ptx in pendingPlatformTxs)
        {
            try
            {
                decimal settledAmount = 0m;
                Guid trackedId = Guid.Empty;

                await unitOfWork.ExecuteInTransactionAsync(async () =>
                {
                    var trackedPtx = await unitOfWork.PlatformWalletTransactionRepository.AsQueryable()
                        .FirstOrDefaultAsync(t => t.Id == ptx.Id);

                    if (trackedPtx == null || trackedPtx.Status != TransactionStatus.Pending)
                        return;

                    var wallet = await unitOfWork.PlatformWalletRepository.AsQueryable()
                        .FirstOrDefaultAsync(w => w.Id == trackedPtx.PlatformWalletId);

                    if (wallet == null)
                    {
                        logger.LogWarning("Platform settlement skipped - wallet not found for tx {TxId}", trackedPtx.Id);
                        return;
                    }

                    var settleAmount = Math.Min(trackedPtx.Amount, wallet.PendingBalance);
                    if (settleAmount <= 0)
                    {
                        logger.LogError("Platform settlement invalid or pending balance insufficient for tx {TxId}", trackedPtx.Id);
                        return;
                    }

                    var availableBefore = wallet.AvailableBalance;
                    wallet.PendingBalance -= settleAmount;
                    wallet.AvailableBalance += settleAmount;
                    wallet.UpdatedAt = DateTime.UtcNow;

                    // Create a completed platform transaction to reflect available balance change
                    var settlementPtx = new PlatformWalletTransaction
                    {
                        PlatformWalletId = wallet.Id,
                        ReferenceId = trackedPtx.ReferenceId,
                        ReferenceType = trackedPtx.ReferenceType,
                        Type = PlatformTransactionType.Revenue,
                        Status = TransactionStatus.Completed,
                        Amount = settleAmount,
                        Currency = trackedPtx.Currency,
                        BalanceBefore = availableBefore,
                        BalanceAfter = wallet.AvailableBalance,
                        Description = $"Settlement release for platform tx {trackedPtx.Id}",
                        CreatedAt = DateTime.UtcNow
                    };

                    await unitOfWork.PlatformWalletTransactionRepository.AddAsync(settlementPtx);

                    // Mark original pending platform tx as completed
                    trackedPtx.Status = TransactionStatus.Completed;
                    trackedPtx.UpdatedAt = DateTime.UtcNow;

                    // capture for logging after commit
                    settledAmount = settleAmount;
                    trackedId = trackedPtx.Id;
                });

                if (settledAmount > 0)
                {
                    logger.LogInformation("Platform transaction settled: {TxId} released {Amount}", trackedId, settledAmount);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to process platform settlement for tx {TxId}", ptx.Id);
            }
        }
    }

    public async Task<ApiResponse<bool>> ForceSettleOrderAsync(Guid orderId)
    {
        // Find transactions for the order and process immediately
        var txs = await unitOfWork.TransactionLedgerRepository.AsQueryable()
            .Where(t => t.ReferenceId == orderId && t.Type == TransactionType.Sale)
            .ToListAsync();

        if (!txs.Any())
            return ApiResponse<bool>.FailureResponse("No transactions found for order");

        foreach (var tx in txs)
        {
            if (tx.Status == TransactionStatus.Completed)
                continue;

            var wallet = await unitOfWork.InstructorWalletRepository.AsQueryable()
                .FirstOrDefaultAsync(w => w.Id == tx.WalletId);
            if (wallet == null)
                continue;

            var settleResult = await walletService.SettleToAvailableAsync(wallet.InstructorId, tx.Amount, orderId, tx.Id, $"Forced settlement for order {orderId}");
            if (!settleResult.IsSuccess)
                return ApiResponse<bool>.FailureResponse(settleResult.Message);

            tx.Status = TransactionStatus.Completed;
            tx.UpdatedAt = DateTime.UtcNow;
        }

        var order = await unitOfWork.OrderRepository.AsQueryable().FirstOrDefaultAsync(o => o.Id == orderId);
        if (order != null)
        {
            order.IsSettled = true;
            order.SettledAt = DateTime.UtcNow;
            order.UpdatedAt = DateTime.UtcNow;
        }

        await unitOfWork.SaveChangesAsync();

        return ApiResponse<bool>.SuccessResponse(true, "Order forced settled");
    }

    // Note: individual instructor/platform upcoming queries were consolidated.
    // See GetUpcomingByOrderAsync and GetMyUpcomingSettlementsAsync instead.

    public async Task<ApiResponse<List<UpcomingSettlementResponse>>> GetMyUpcomingSettlementsAsync(Guid instructorId, PaginationRequest pagination)
    {
        var query = unitOfWork.TransactionLedgerRepository.AsQueryable()
            .Where(t => t.Type == TransactionType.Sale && t.Status == TransactionStatus.Pending && t.AvailableAt != null && t.InstructorWallet != null && t.InstructorWallet.InstructorId == instructorId);

        var total = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.AvailableAt)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(t => new UpcomingSettlementResponse
            {
                TransactionId = t.Id,
                WalletId = t.WalletId,
                OrderId = t.ReferenceId,
                Amount = t.Amount,
                Currency = t.Currency,
                AvailableAt = t.AvailableAt,
                CreatedAt = t.CreatedAt,
                Status = t.Status
            })
            .ToListAsync();

        return ApiResponse<List<UpcomingSettlementResponse>>.SuccessPagedResponse(
            items,
            total,
            pagination.PageNumber,
            pagination.PageSize,
            "Upcoming settlements for instructor retrieved");
    }

    public async Task<ApiResponse<List<UpcomingByOrderResponse>>> GetUpcomingByOrderAsync(DateTime? from, DateTime? to, PaginationRequest pagination)
    {
        // Load pending instructor transactions (by order)
        var instructorQuery = unitOfWork.TransactionLedgerRepository.AsQueryable()
            .Where(t => t.Type == TransactionType.Sale && t.Status == TransactionStatus.Pending && t.AvailableAt != null && t.ReferenceId != null);

        if (from.HasValue) instructorQuery = instructorQuery.Where(t => t.AvailableAt >= from.Value);
        if (to.HasValue) instructorQuery = instructorQuery.Where(t => t.AvailableAt <= to.Value);

        var instructorTxs = await instructorQuery
            .Select(t => new { OrderId = t.ReferenceId!.Value, t.Amount, t.AvailableAt, t.Status })
            .ToListAsync();

        // Load platform revenue transactions (by order reference).
        // Only include Revenue type — CouponCost transactions are immediate debits, not part of settlement.
        var platformQuery = unitOfWork.PlatformWalletTransactionRepository.AsQueryable()
            .Where(t => t.ReferenceId != null
                && t.Type == PlatformTransactionType.Revenue
                && !(t.Description != null && t.Description.Contains("Settlement release for platform tx")));

        // Use effective available time = AvailableAt (if set) otherwise CreatedAt for immediate credits
        if (from.HasValue) platformQuery = platformQuery.Where(t => (t.AvailableAt ?? t.CreatedAt) >= from.Value);
        if (to.HasValue) platformQuery = platformQuery.Where(t => (t.AvailableAt ?? t.CreatedAt) <= to.Value);

        var platformTxs = await platformQuery
            .Select(t => new { OrderId = t.ReferenceId!.Value, t.Amount, EffectiveAt = (DateTime?)(t.AvailableAt ?? t.CreatedAt), t.Status })
            .ToListAsync();

        // Group by order
        var orderIds = instructorTxs.Select(x => x.OrderId).Union(platformTxs.Select(x => x.OrderId)).Distinct().ToList();

        var groups = orderIds
            .Select(id =>
            {
                var ins = instructorTxs.Where(x => x.OrderId == id).ToList();
                var plat = platformTxs.Where(x => x.OrderId == id).ToList();

                {
                    var insMin = ins.Any() ? ins.Min(x => x.AvailableAt) : (DateTime?)null;
                    var platMin = plat.Any() ? plat.Min(x => x.EffectiveAt) : (DateTime?)null;
                    var availableAt = insMin.HasValue && platMin.HasValue
                        ? (insMin.Value <= platMin.Value ? insMin : platMin)
                        : insMin ?? platMin;

                    return new UpcomingByOrderResponse
                    {
                        OrderId = id,
                        InstructorAmount = ins.Sum(x => x.Amount),
                        PlatformAmount = plat.Sum(x => x.Amount),
                        AvailableAt = availableAt,
                        Currency = "VND",
                        InstructorStatus = ins.Any() ? (ins.All(x => x.Status == TransactionStatus.Completed) ? TransactionStatus.Completed : TransactionStatus.Pending) : (TransactionStatus?)null,
                        PlatformStatus = plat.Any() ? (plat.All(x => x.Status == TransactionStatus.Completed) ? TransactionStatus.Completed : TransactionStatus.Pending) : (TransactionStatus?)null
                    };
                }
            })
            .OrderBy(x => x.AvailableAt ?? DateTime.MaxValue)
            .ToList();

        var total = groups.Count;

        var items = groups
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToList();

        return ApiResponse<List<UpcomingByOrderResponse>>.SuccessPagedResponse(items, total, pagination.PageNumber, pagination.PageSize, "Upcoming settlements by order retrieved");
    }
}
