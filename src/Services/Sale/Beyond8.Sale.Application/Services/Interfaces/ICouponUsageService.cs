using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.CouponUsages;

namespace Beyond8.Sale.Application.Services.Interfaces;

/// <summary>
/// Coupon usage tracking and validation logic
/// </summary>
public interface ICouponUsageService
{
    /// <summary>
    /// Validate if coupon can be used by user for specific courses
    /// Checks: expiry, usage limits (global + per-user), applicability, min order amount
    /// </summary>
    Task<ApiResponse<CouponValidationResult>> ValidateCouponAsync(
        string code,
        Guid userId,
        List<Guid> courseIds,
        decimal orderSubtotal);

    /// <summary>
    /// Record coupon usage when order is completed
    /// </summary>
    Task<ApiResponse<bool>> RecordUsageAsync(CreateCouponUsageRequest request);

    /// <summary>
    /// Get how many times user has used specific coupon
    /// </summary>
    Task<ApiResponse<int>> GetUserUsageCountAsync(Guid userId, Guid couponId);

    /// <summary>
    /// Get user's coupon usage history
    /// </summary>
    Task<ApiResponse<List<CouponUsageResponse>>> GetUserUsageHistoryAsync(
        Guid userId,
        PaginationRequest pagination);

    /// <summary>
    /// Get all coupon usages for specific order
    /// </summary>
    Task<ApiResponse<CouponUsageResponse>> GetUsageByOrderAsync(Guid orderId);

    /// <summary>
    /// Check if user can use coupon (without full validation)
    /// Quick check for UI display
    /// </summary>
    Task<ApiResponse<bool>> CanUserUseCouponAsync(Guid userId, string couponCode);
}
