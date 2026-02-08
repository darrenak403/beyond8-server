using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.CouponUsages;

namespace Beyond8.Sale.Application.Services.Interfaces;

/// <summary>
/// Coupon usage tracking and validation logic
/// </summary>
public interface ICouponUsageService
{
    // ── Internal (called by OrderService / PaymentService) ──
    Task<ApiResponse<CouponValidationResult>> ValidateCouponAsync(
        string code, Guid userId, List<Guid> courseIds, decimal orderSubtotal);
    Task<ApiResponse<bool>> RecordUsageAsync(CreateCouponUsageRequest request);
    Task<ApiResponse<int>> GetUserUsageCountAsync(Guid userId, Guid couponId);
    Task<ApiResponse<CouponUsageResponse>> GetUsageByOrderAsync(Guid orderId);

    // ── Student ──
    Task<ApiResponse<List<CouponUsageResponse>>> GetUserUsageHistoryAsync(
        Guid userId, PaginationRequest pagination);

    // ── Public (UI quick check) ──
    Task<ApiResponse<bool>> CanUserUseCouponAsync(Guid userId, string couponCode);
}
