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
    Task<ApiResponse<decimal>> ApplyCouponAsync(string code, decimal orderTotal);
}