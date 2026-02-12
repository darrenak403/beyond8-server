using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.CouponUsages;
using Beyond8.Sale.Application.Mappings.CouponUsages;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

public class CouponUsageService(
    ILogger<CouponUsageService> logger,
    IUnitOfWork unitOfWork) : ICouponUsageService
{
    public async Task<ApiResponse<CouponValidationResult>> ValidateCouponAsync(
        string code, Guid userId, List<Guid> courseIds, decimal orderSubtotal)
    {
        var coupon = await unitOfWork.CouponRepository
            .FindOneAsync(c => c.Code.ToUpper() == code.ToUpper() && c.IsActive);

        if (coupon == null)
            return InvalidCouponResult("Coupon không tồn tại hoặc đã bị vô hiệu hóa");

        // Check basic eligibility (expiry + global usage limit)
        var eligibilityError = ValidateBasicEligibility(coupon);
        if (eligibilityError != null)
            return InvalidCouponResult(eligibilityError);

        // Check minimum order amount
        if (coupon.MinOrderAmount.HasValue && orderSubtotal < coupon.MinOrderAmount.Value)
            return InvalidCouponResult($"Đơn hàng tối thiểu {coupon.MinOrderAmount:N0} VND để áp dụng coupon");

        // Check per-user usage limit
        if (coupon.UsagePerUser.HasValue)
        {
            var usageCount = await CountUserUsageAsync(userId, coupon.Id);
            if (usageCount >= coupon.UsagePerUser.Value)
                return InvalidCouponResult("Bạn đã sử dụng coupon này quá số lần cho phép");
        }

        // Check course applicability
        if (coupon.ApplicableCourseId.HasValue && !courseIds.Contains(coupon.ApplicableCourseId.Value))
            return InvalidCouponResult("Coupon không áp dụng cho các khóa học trong đơn hàng");

        // TODO: Check instructor applicability when Catalog service integration is ready

        var discountAmount = CalculateDiscount(coupon, orderSubtotal);

        return ApiResponse<CouponValidationResult>.SuccessResponse(
            new CouponValidationResult
            {
                IsValid = true,
                CouponId = coupon.Id,
                CouponCode = coupon.Code,
                CouponType = coupon.Type.ToString(),
                DiscountValue = coupon.Value,
                MaxDiscountAmount = coupon.MaxDiscountAmount,
                CalculatedDiscount = discountAmount,
                IsActive = coupon.IsActive,
                ValidFrom = coupon.ValidFrom,
                ValidUntil = coupon.ValidTo,
                UsageLimit = coupon.UsageLimit,
                CurrentUsageCount = coupon.UsedCount,
                UsageLimitPerUser = coupon.UsagePerUser,
                IsApplicableToCart = true
            },
            "Coupon hợp lệ");
    }

    public async Task<ApiResponse<bool>> RecordUsageAsync(CreateCouponUsageRequest request)
    {
        // Get tracked coupon for UsedCount update
        var coupon = await unitOfWork.CouponRepository.AsQueryable()
            .FirstOrDefaultAsync(c => c.Id == request.CouponId && c.IsActive);

        if (coupon == null)
            return ApiResponse<bool>.FailureResponse("Coupon không tồn tại hoặc đã bị vô hiệu hóa");

        var usage = request.ToEntity();
        await unitOfWork.CouponUsageRepository.AddAsync(usage);

        // Update coupon usage count (tracked entity — changes will be saved)
        coupon.UsedCount += 1;
        coupon.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Coupon usage recorded: {CouponId} for user {UserId}, order {OrderId}",
            request.CouponId, request.UserId, request.OrderId);

        return ApiResponse<bool>.SuccessResponse(true, "Ghi nhận sử dụng coupon thành công");
    }

    public async Task<ApiResponse<int>> GetUserUsageCountAsync(Guid userId, Guid couponId)
    {
        var count = await CountUserUsageAsync(userId, couponId);
        return ApiResponse<int>.SuccessResponse(count, "Lấy số lần sử dụng thành công");
    }

    public async Task<ApiResponse<List<CouponUsageResponse>>> GetUserUsageHistoryAsync(
        Guid userId, PaginationRequest pagination)
    {
        var usages = await unitOfWork.CouponUsageRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: u => u.UserId == userId,
            orderBy: q => q.OrderByDescending(u => u.UsedAt),
            includes: q => q.Include(u => u.Coupon).Include(u => u.Order));

        var responses = usages.Items.Select(u => u.ToResponse()).ToList();

        return ApiResponse<List<CouponUsageResponse>>.SuccessPagedResponse(
            responses, usages.TotalCount, pagination.PageNumber, pagination.PageSize,
            "Lấy lịch sử sử dụng coupon thành công");
    }

    public async Task<ApiResponse<CouponUsageResponse>> GetUsageByOrderAsync(Guid orderId)
    {
        var usage = await unitOfWork.CouponUsageRepository.AsQueryable()
            .Include(u => u.Coupon)
            .Include(u => u.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.OrderId == orderId);

        if (usage == null)
            return ApiResponse<CouponUsageResponse>.FailureResponse(
                "Không tìm thấy thông tin sử dụng coupon cho đơn hàng này");

        return ApiResponse<CouponUsageResponse>.SuccessResponse(
            usage.ToResponse(), "Lấy thông tin coupon usage thành công");
    }

    public async Task<ApiResponse<bool>> CanUserUseCouponAsync(Guid userId, string couponCode)
    {
        var coupon = await unitOfWork.CouponRepository
            .FindOneAsync(c => c.Code.ToUpper() == couponCode.ToUpper() && c.IsActive);

        if (coupon == null)
            return ApiResponse<bool>.SuccessResponse(false, "Coupon không hợp lệ");

        // Check basic eligibility (expiry + global usage limit)
        var eligibilityError = ValidateBasicEligibility(coupon);
        if (eligibilityError != null)
            return ApiResponse<bool>.SuccessResponse(false, eligibilityError);

        // Check per-user usage limit
        if (coupon.UsagePerUser.HasValue)
        {
            var usageCount = await CountUserUsageAsync(userId, coupon.Id);
            if (usageCount >= coupon.UsagePerUser.Value)
                return ApiResponse<bool>.SuccessResponse(false, "Đã đạt giới hạn sử dụng cho user này");
        }

        return ApiResponse<bool>.SuccessResponse(true, "User có thể sử dụng coupon");
    }

    // ── Private Helpers ──

    private async Task<int> CountUserUsageAsync(Guid userId, Guid couponId)
    {
        return await unitOfWork.CouponUsageRepository.AsQueryable()
            .CountAsync(u => u.UserId == userId && u.CouponId == couponId);
    }

    private static string? ValidateBasicEligibility(Coupon coupon)
    {
        var now = DateTime.UtcNow;

        if (now < coupon.ValidFrom || now > coupon.ValidTo)
            return "Coupon đã hết hạn sử dụng";

        if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit.Value)
            return "Coupon đã hết lượt sử dụng";

        return null;
    }

    private static ApiResponse<CouponValidationResult> InvalidCouponResult(string errorMessage)
    {
        return ApiResponse<CouponValidationResult>.SuccessResponse(
            new CouponValidationResult { IsValid = false, ErrorMessage = errorMessage },
            "Validation failed");
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
