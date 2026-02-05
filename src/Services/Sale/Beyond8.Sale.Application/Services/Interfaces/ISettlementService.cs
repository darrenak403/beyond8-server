using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Dtos.Settlements;

namespace Beyond8.Sale.Application.Services.Interfaces;

/// <summary>
/// 14-day escrow settlement logic - move funds from PendingBalance to AvailableBalance
/// </summary>
public interface ISettlementService
{
    /// <summary>
    /// Process all orders eligible for settlement (SettlementEligibleAt <= Now)
    /// Run by background job daily/hourly
    /// </summary>
    Task<ApiResponse<int>> ProcessPendingSettlementsAsync();

    /// <summary>
    /// Settle specific order (manual/triggered)
    /// </summary>
    Task<ApiResponse<bool>> SettleOrderAsync(Guid orderId);

    /// <summary>
    /// Get all orders pending settlement (PaidAt not null, IsSettled = false)
    /// </summary>
    Task<ApiResponse<List<OrderResponse>>> GetPendingSettlementsAsync(PaginationRequest pagination);

    /// <summary>
    /// Get settlement status and details for specific order
    /// </summary>
    Task<ApiResponse<SettlementStatusResponse>> GetSettlementStatusAsync(Guid orderId);

    /// <summary>
    /// Force settle order immediately (Admin only - emergency use)
    /// </summary>
    Task<ApiResponse<bool>> ForceSettleAsync(Guid orderId, string reason);

    /// <summary>
    /// Get orders becoming eligible for settlement within next N days
    /// </summary>
    Task<ApiResponse<List<OrderResponse>>> GetUpcomingSettlementsAsync(int daysAhead, PaginationRequest pagination);

    /// <summary>
    /// Get settlement statistics for reporting
    /// </summary>
    Task<ApiResponse<SettlementStatisticsResponse>> GetSettlementStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
}
