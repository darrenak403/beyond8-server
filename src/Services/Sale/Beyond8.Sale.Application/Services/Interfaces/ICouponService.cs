using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Coupons;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface ICouponService
{
    Task<ApiResponse<CouponResponse>> CreateCouponAsync(CreateCouponRequest request);
    Task<ApiResponse<CouponResponse>> GetCouponByCodeAsync(string code);
    Task<ApiResponse<CouponResponse>> UpdateCouponAsync(Guid couponId, UpdateCouponRequest request);
    Task<ApiResponse<bool>> DeleteCouponAsync(Guid couponId);
    Task<ApiResponse<List<CouponResponse>>> GetCouponsAsync(PaginationRequest pagination);
    Task<ApiResponse<List<CouponResponse>>> GetActiveCouponsAsync();
    Task<ApiResponse<List<CouponResponse>>> GetCouponsByInstructorAsync(Guid instructorId);
    Task<ApiResponse<bool>> ToggleCouponStatusAsync(Guid couponId);
    Task<ApiResponse<decimal>> ApplyCouponAsync(string code, decimal orderTotal);
    Task<ApiResponse<(bool IsValid, string? ErrorMessage, decimal DiscountAmount, Guid? CouponId)>> ValidateAndApplyCouponAsync(string code, decimal orderTotal, List<Guid> courseIds);
}