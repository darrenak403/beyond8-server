using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Application.Mappings.Coupons;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class CouponService(
    ILogger<CouponService> logger,
    IUnitOfWork unitOfWork) : ICouponService
{
    public async Task<ApiResponse<CouponResponse>> CreateCouponAsync(CreateCouponRequest request)
    {
        try
        {
            var existingCoupon = await unitOfWork.CouponRepository
                .FindOneAsync(c => c.Code.ToUpper() == request.Code.ToUpper());

            if (existingCoupon != null)
                return ApiResponse<CouponResponse>.FailureResponse("Mã coupon đã tồn tại");

            var coupon = request.ToEntity();

            await unitOfWork.CouponRepository.AddAsync(coupon);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Coupon created: {Code}", coupon.Code);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Tạo coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating coupon");
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi tạo coupon");
        }
    }

    public async Task<ApiResponse<CouponResponse>> GetCouponByCodeAsync(string code)
    {
        try
        {
            var coupon = await unitOfWork.CouponRepository
                        .FindOneAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

            if (coupon == null)
                return ApiResponse<CouponResponse>.FailureResponse("Coupon không tồn tại hoặc đã bị vô hiệu hóa");

            var eligibilityError = ValidateBasicEligibility(coupon);
            if (eligibilityError != null)
                return ApiResponse<CouponResponse>.FailureResponse(eligibilityError);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Lấy thông tin coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving coupon by code: {Code}", code);
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi lấy coupon");
        }
    }

    public async Task<ApiResponse<CouponResponse>> UpdateCouponAsync(Guid couponId, UpdateCouponRequest request)
    {
        try
        {
            // Use AsQueryable for tracked entity (required for SaveChangesAsync)
            var coupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<CouponResponse>.FailureResponse("Coupon không tồn tại");

            coupon.UpdateFrom(request);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Coupon updated: {CouponId}", couponId);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Cập nhật coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating coupon: {CouponId}", couponId);
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật coupon");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCouponAsync(Guid couponId)
    {
        try
        {
            // Use AsQueryable for tracked entity (required for SaveChangesAsync)
            var coupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<bool>.FailureResponse("Coupon không tồn tại");

            if (coupon.UsedCount > 0)
                return ApiResponse<bool>.FailureResponse("Không thể xóa coupon đã được sử dụng");

            coupon.IsActive = false;
            coupon.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Coupon deactivated: {CouponId}", couponId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting coupon: {CouponId}", couponId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa coupon");
        }
    }

    public async Task<ApiResponse<List<CouponResponse>>> GetCouponsAsync(PaginationRequest pagination)
    {
        try
        {
            var coupons = await unitOfWork.CouponRepository.GetPagedAsync(
              pageNumber: pagination.PageNumber,
              pageSize: pagination.PageSize,
              orderBy: q => q.OrderByDescending(c => c.CreatedAt));

            var responses = coupons.Items.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CouponResponse>>.SuccessPagedResponse(
                responses, coupons.TotalCount, pagination.PageNumber, pagination.PageSize,
                "Lấy danh sách coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving coupons");
            return ApiResponse<List<CouponResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách coupon");
        }
    }

    public async Task<ApiResponse<List<CouponResponse>>> GetActiveCouponsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var coupons = await unitOfWork.CouponRepository.AsQueryable()
                .Where(c => c.IsActive
                    && c.ValidFrom <= now
                    && c.ValidTo >= now
                    && (c.UsageLimit == null || c.UsedCount < c.UsageLimit.Value))
                .OrderByDescending(c => c.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var responses = coupons.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CouponResponse>>.SuccessResponse(
                responses, "Lấy danh sách coupon active thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving active coupons");
            return ApiResponse<List<CouponResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách coupon active");
        }

    }

    public async Task<ApiResponse<List<CouponResponse>>> GetCouponsByInstructorAsync(Guid instructorId)
    {
        try
        {
            var coupons = await unitOfWork.CouponRepository.AsQueryable()
               .Where(c => c.ApplicableInstructorId == instructorId)
               .OrderByDescending(c => c.CreatedAt)
               .AsNoTracking()
               .ToListAsync();

            var responses = coupons.Select(c => c.ToResponse()).ToList();

            return ApiResponse<List<CouponResponse>>.SuccessResponse(
                responses, "Lấy danh sách coupon của instructor thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving coupons for instructor: {InstructorId}", instructorId);
            return ApiResponse<List<CouponResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách coupon của instructor");
        }
    }

    public async Task<ApiResponse<bool>> ToggleCouponStatusAsync(Guid couponId)
    {
        try
        {
            // Use AsQueryable for tracked entity (required for SaveChangesAsync)
            var coupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<bool>.FailureResponse("Coupon không tồn tại");

            coupon.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            var statusText = coupon.IsActive ? "kích hoạt" : "vô hiệu hóa";
            logger.LogInformation("Coupon status toggled: {CouponId} -> {IsActive}", couponId, coupon.IsActive);

            return ApiResponse<bool>.SuccessResponse(coupon.IsActive, $"Coupon đã được {statusText}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling coupon status: {CouponId}", couponId);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi thay đổi trạng thái coupon");
        }
    }

    public async Task<ApiResponse<decimal>> ApplyCouponAsync(string code, decimal orderTotal)
    {
        try
        {
            var coupon = await unitOfWork.CouponRepository
               .FindOneAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

            if (coupon == null)
                return ApiResponse<decimal>.FailureResponse("Coupon không tồn tại hoặc đã bị vô hiệu hóa");

            var eligibilityError = ValidateBasicEligibility(coupon);
            if (eligibilityError != null)
                return ApiResponse<decimal>.FailureResponse(eligibilityError);

            if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount.Value)
                return ApiResponse<decimal>.FailureResponse(
                    $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0} VND để áp dụng coupon");

            var discount = CalculateDiscount(coupon, orderTotal);

            return ApiResponse<decimal>.SuccessResponse(discount, "Áp dụng coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error applying coupon: {Code}", code);
            return ApiResponse<decimal>.FailureResponse("Đã xảy ra lỗi khi áp dụng coupon");
        }
    }

    public async Task<ApiResponse<(bool IsValid, string? ErrorMessage, decimal DiscountAmount, Guid? CouponId)>> ValidateAndApplyCouponAsync(
        string code, decimal orderTotal, List<Guid> courseIds)
    {
        try
        {
            var coupon = await unitOfWork.CouponRepository
            .FindOneAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

            if (coupon == null)
                return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                    (false, "Coupon không tồn tại hoặc đã bị vô hiệu hóa", 0, null), "Validation failed");

            var eligibilityError = ValidateBasicEligibility(coupon);
            if (eligibilityError != null)
                return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                    (false, eligibilityError, 0, null), "Validation failed");

            if (coupon.MinOrderAmount.HasValue && orderTotal < coupon.MinOrderAmount.Value)
                return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                    (false, $"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0} VND để áp dụng coupon", 0, null), "Validation failed");

            // Check course applicability
            if (coupon.ApplicableCourseId.HasValue && !courseIds.Any(id => id == coupon.ApplicableCourseId.Value))
                return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                    (false, "Coupon không áp dụng cho các khóa học trong đơn hàng", 0, null), "Validation failed");

            // TODO: Check instructor applicability when Catalog service integration is ready

            var discount = CalculateDiscount(coupon, orderTotal);

            return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                (true, null, discount, coupon.Id), "Coupon hợp lệ");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validating and applying coupon: {Code}", code);
            return ApiResponse<(bool, string?, decimal, Guid?)>.FailureResponse("Đã xảy ra lỗi khi xác thực và áp dụng coupon");
        }
    }

    // ── Private Helpers ──

    private static string? ValidateBasicEligibility(Coupon coupon)
    {
        var now = DateTime.UtcNow;

        if (now < coupon.ValidFrom || now > coupon.ValidTo)
            return "Coupon đã hết hạn sử dụng";

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            return "Coupon đã hết lượt sử dụng";

        return null;
    }

    private static decimal CalculateDiscount(Coupon coupon, decimal orderTotal)
    {
        if (coupon.Type == CouponType.Percentage)
        {
            var discount = orderTotal * (coupon.Value / 100);
            return coupon.MaxDiscountAmount.HasValue
                ? Math.Min(discount, coupon.MaxDiscountAmount.Value)
                : discount;
        }

        if (coupon.Type == CouponType.FixedAmount)
            return Math.Min(coupon.Value, orderTotal);

        return 0;
    }
}
