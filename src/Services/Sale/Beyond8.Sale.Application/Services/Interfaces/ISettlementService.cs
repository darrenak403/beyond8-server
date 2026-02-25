using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Settlements;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface ISettlementService
{
    /// <summary>
    /// Process pending settlements that are eligible (AvailableAt <= now).
    /// Should be idempotent and safe to run concurrently (DB locking required).
    /// </summary>
    Task<ApiResponse<bool>> ProcessPendingSettlementsAsync();

    /// <summary>
    /// Force settle a single order (admin override).
    /// </summary>
    Task<ApiResponse<bool>> ForceSettleOrderAsync(Guid orderId);
    
    Task<ApiResponse<List<UpcomingSettlementResponse>>> GetMyUpcomingSettlementsAsync(Guid instructorId, PaginationRequest pagination);

    /// <summary>
    /// Get upcoming settlements grouped by order: returns instructor and platform amounts per order.
    /// Admin only.
    /// </summary>
    Task<ApiResponse<List<UpcomingByOrderResponse>>> GetUpcomingByOrderAsync(DateTime? from, DateTime? to, PaginationRequest pagination);

    /// <summary>
    /// Background entry point to run pending settlement processing once.
    /// Intended to be called by schedulers (Hangfire) or on-demand.
    /// </summary>
}
