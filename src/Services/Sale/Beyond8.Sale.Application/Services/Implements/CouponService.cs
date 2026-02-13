using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Application.Mappings.Coupons;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Beyond8.Common.Security;

namespace Beyond8.Sale.Application.Services.Implements;

public class CouponService(
    ILogger<CouponService> logger,
    IUnitOfWork unitOfWork,
    IInstructorWalletService walletService,
    ICatalogClient catalogClient,
    ICurrentUserService currentUserService) : ICouponService
{
    public async Task<ApiResponse<CouponResponse>> CreateAdminCouponAsync(CreateAdminCouponRequest request)
    {
        try
        {
            var existingCoupon = await unitOfWork.CouponRepository
                .FindOneAsync(c => c.Code.ToUpper() == request.Code.ToUpper());

            if (existingCoupon != null)
                return ApiResponse<CouponResponse>.FailureResponse("Mã coupon đã tồn tại");

            var coupon = request.ToEntity();

            // Admin coupons don't hold funds (no instructor ownership)
            await unitOfWork.CouponRepository.AddAsync(coupon);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Admin coupon created: {Code}", coupon.Code);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Tạo coupon admin thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating admin coupon");
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi tạo coupon admin");
        }
    }

    public async Task<ApiResponse<CouponResponse>> CreateInstructorCouponAsync(CreateInstructorCouponRequest request, Guid instructorId)
    {
        try
        {
            var existingCoupon = await unitOfWork.CouponRepository
                .FindOneAsync(c => c.Code.ToUpper() == request.Code.ToUpper());

            if (existingCoupon != null)
                return ApiResponse<CouponResponse>.FailureResponse("Mã coupon đã tồn tại");

            var coupon = request.ToEntity();
            coupon.ApplicableInstructorId = request.InstructorId; // Use InstructorId from request instead of parameter

            // ── Get course price for hold amount calculation ──
            var courseResult = await catalogClient.GetCourseByIdAsync(request.ApplicableCourseId);
            if (!courseResult.IsSuccess || courseResult.Data == null)
                return ApiResponse<CouponResponse>.FailureResponse("Khóa học không tồn tại");

            var coursePrice = courseResult.Data.OriginalPrice;

            // ── Instructor coupon: Hold funds from wallet ──
            var holdAmount = CalculateCouponHoldAmount(request, coursePrice);

            // For percentage coupons without MaxDiscountAmount, allow creation without holding funds
            if (holdAmount < 0)
                return ApiResponse<CouponResponse>.FailureResponse(
                    "Không thể tính toán số tiền cần giữ. Vui lòng kiểm tra UsageLimit và MaxDiscountAmount/Value");

            // Only hold funds if there's an amount to hold
            if (holdAmount > 0)
            {
                var holdResult = await walletService.HoldFundsForCouponAsync(
                    request.InstructorId,
                    holdAmount,
                    coupon.Id,
                    $"Giữ tiền cho coupon {coupon.Code} ({request.UsageLimit} lần sử dụng)");

                if (!holdResult.IsSuccess)
                    return ApiResponse<CouponResponse>.FailureResponse(holdResult.Message ?? "Không thể giữ tiền từ ví");


                coupon.HoldAmount = holdAmount;
                coupon.RemainingHoldAmount = holdAmount;
            }
            else
            {
                // No funds to hold (percentage coupon without MaxDiscountAmount)
                coupon.HoldAmount = 0;
                coupon.RemainingHoldAmount = 0;
            }

            await unitOfWork.CouponRepository.AddAsync(coupon);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Instructor coupon created: {Code}, HoldAmount: {HoldAmount}", coupon.Code, coupon.HoldAmount);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Tạo coupon instructor thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating instructor coupon");
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi tạo coupon instructor");
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

    public async Task<ApiResponse<CouponResponse>> UpdateCouponAsync(Guid couponId, UpdateCouponRequest request, Guid userId)
    {
        try
        {
            // Get coupon with tracking for updates
            var coupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<CouponResponse>.FailureResponse("Coupon không tồn tại");

            // Check business rules
            var businessRuleError = ValidateBusinessRulesForUpdate(coupon);
            if (businessRuleError != null)
                return ApiResponse<CouponResponse>.FailureResponse(businessRuleError);

            // Check authorization
            if (!HasPermissionToUpdateCoupon(coupon, userId))
                return ApiResponse<CouponResponse>.FailureResponse("Không có quyền cập nhật coupon này");

            // Store old values for logging
            var oldCode = coupon.Code;
            var oldIsActive = coupon.IsActive;

            // Apply updates
            coupon.UpdateFrom(request);
            coupon.UpdatedAt = DateTime.UtcNow;

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation(
                "Coupon updated: {CouponId} (Code: {OldCode} -> {NewCode}, Active: {OldActive} -> {NewActive}) by user: {UserId}",
                couponId, oldCode, coupon.Code, oldIsActive, coupon.IsActive, userId);

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Cập nhật coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating coupon: {CouponId} by user: {UserId}", couponId, userId);
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật coupon");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCouponAsync(Guid couponId, Guid userId)
    {
        try
        {
            // Use AsQueryable for tracked entity (required for SaveChangesAsync)
            var coupon = await unitOfWork.CouponRepository.AsQueryable()
                .FirstOrDefaultAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<bool>.FailureResponse("Coupon không tồn tại");

            // Check authorization: Admin can only delete admin coupons, Instructor can only delete their own coupons
            bool hasPermission = false;
            if (coupon.ApplicableInstructorId == null)
            {
                // Admin coupon - only admin can delete
                hasPermission = currentUserService.IsInRole(Role.Admin);
            }
            else
            {
                // Instructor coupon - only the owner instructor can delete
                hasPermission = coupon.ApplicableInstructorId == userId;
            }

            if (!hasPermission)
                return ApiResponse<bool>.FailureResponse("Không có quyền xóa coupon này");

            if (coupon.UsedCount > 0)
                return ApiResponse<bool>.FailureResponse("Không thể xóa coupon đã được sử dụng");

            // ── Release held funds back to instructor wallet ──
            if (coupon.ApplicableInstructorId.HasValue && coupon.RemainingHoldAmount > 0)
            {
                await walletService.ReleaseCouponHoldAsync(
                    coupon.ApplicableInstructorId.Value,
                    coupon.RemainingHoldAmount,
                    coupon.Id,
                    $"Hoàn trả tiền giữ do xóa coupon {coupon.Code}");

                coupon.RemainingHoldAmount = 0;
            }

            coupon.IsActive = false;
            coupon.UpdatedAt = DateTime.UtcNow;
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Coupon deactivated: {CouponId} by user: {UserId}", couponId, userId);

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

            // Toggle the status
            coupon.IsActive = !coupon.IsActive;
            coupon.UpdatedAt = DateTime.UtcNow;

            // ── Release held funds when deactivating instructor coupon ──
            if (!coupon.IsActive && coupon.ApplicableInstructorId.HasValue && coupon.RemainingHoldAmount > 0)
            {
                await walletService.ReleaseCouponHoldAsync(
                    coupon.ApplicableInstructorId.Value,
                    coupon.RemainingHoldAmount,
                    coupon.Id,
                    $"Hoàn trả tiền giữ do vô hiệu hóa coupon {coupon.Code}");

                coupon.RemainingHoldAmount = 0;
            }

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
        string code, decimal orderTotal, List<Guid> courseIds, Guid? userId = null)
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

            // ── Per-user usage limit check (Bug fix #3) ──
            if (userId.HasValue && coupon.UsagePerUser.HasValue)
            {
                var userUsageCount = await unitOfWork.CouponUsageRepository.AsQueryable()
                    .CountAsync(u => u.UserId == userId.Value && u.CouponId == coupon.Id);

                if (userUsageCount >= coupon.UsagePerUser.Value)
                    return ApiResponse<(bool, string?, decimal, Guid?)>.SuccessResponse(
                        (false, "Bạn đã sử dụng coupon này quá số lần cho phép", 0, null), "Validation failed");
            }

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

    // ── Private Helper Methods ──

    private static string? ValidateBusinessRulesForUpdate(Coupon coupon)
    {
        // Business rule: Don't allow updating expired coupons
        if (coupon.ValidTo < DateTime.UtcNow)
            return "Không thể cập nhật coupon đã hết hạn";

        // Business rule: Don't allow updating coupons that have been used if it would break usage limits
        // This is a business decision - for now, allow updates but log warnings

        return null;
    }

    private bool HasPermissionToUpdateCoupon(Coupon coupon, Guid userId)
    {
        if (coupon.ApplicableInstructorId == null)
        {
            // Admin coupon - only admin can update
            return currentUserService.IsInRole(Role.Admin);
        }
        else
        {
            // Instructor coupon - only the owner instructor can update
            return coupon.ApplicableInstructorId == userId;
        }
    }

    /// <summary>
    /// Calculate total hold amount for instructor coupon.
    /// FixedAmount: Value × UsageLimit
    /// Percentage: (CoursePrice × Percentage/100) × UsageLimit
    /// </summary>
    private static decimal CalculateCouponHoldAmount(CreateInstructorCouponRequest request, decimal coursePrice)
    {
        if (!request.UsageLimit.HasValue || request.UsageLimit.Value <= 0)
            return 0;

        if (request.Type == CouponType.FixedAmount)
            return request.Value * request.UsageLimit.Value;

        if (request.Type == CouponType.Percentage)
        {
            // Calculate potential discount per usage: CoursePrice × (Percentage/100)
            var discountPerUsage = coursePrice * (request.Value / 100);

            // If MaxDiscountAmount is set, use the minimum of calculated discount and max discount
            if (request.MaxDiscountAmount.HasValue && request.MaxDiscountAmount.Value > 0)
                discountPerUsage = Math.Min(discountPerUsage, request.MaxDiscountAmount.Value);

            return discountPerUsage * request.UsageLimit.Value;
        }

        return 0;
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

    public async Task<ApiResponse<CouponResponse>> GetCouponByIdAsync(Guid couponId)
    {
        try
        {
            var coupon = await unitOfWork.CouponRepository
                .FindOneAsync(c => c.Id == couponId);

            if (coupon == null)
                return ApiResponse<CouponResponse>.FailureResponse("Coupon không tồn tại");

            return ApiResponse<CouponResponse>.SuccessResponse(
                coupon.ToResponse(), "Lấy thông tin coupon thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving coupon by ID: {CouponId}", couponId);
            return ApiResponse<CouponResponse>.FailureResponse("Đã xảy ra lỗi khi lấy coupon");
        }
    }
    /// <summary>
    /// Validate basic coupon eligibility (active status, validity period, usage limits)
    /// Returns error message if invalid, null if valid
    /// </summary>
    private static string? ValidateBasicEligibility(Coupon coupon)
    {
        if (!coupon.IsActive)
            return "Coupon đã bị vô hiệu hóa";

        var now = DateTime.UtcNow;
        if (now < coupon.ValidFrom)
            return "Coupon chưa có hiệu lực";

        if (now > coupon.ValidTo)
            return "Coupon đã hết hạn";

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            return "Coupon đã đạt giới hạn sử dụng";

        return null; // Valid
    }
}
