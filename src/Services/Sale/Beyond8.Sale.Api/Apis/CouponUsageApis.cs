using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.CouponUsages;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class CouponUsageApis
{
    public static IEndpointRouteBuilder MapCouponUsageApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/coupon-usages")
            .MapCouponUsageRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Coupon Usage Api");

        return builder;
    }

    public static RouteGroupBuilder MapCouponUsageRoutes(this RouteGroupBuilder group)
    {
        // ── Student ──
        group.MapGet("/history", GetUserUsageHistoryAsync)
            .WithName("GetUserCouponUsageHistory")
            .WithDescription("Lấy lịch sử sử dụng coupon của người dùng (Student, paginated)")
            .RequireAuthorization()
            .Produces<ApiResponse<List<CouponUsageResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // ── Public (UI quick check) ──
        group.MapGet("/can-use", CanUserUseCouponAsync)
            .WithName("CanUserUseCoupon")
            .WithDescription("Kiểm tra người dùng có thể sử dụng coupon không (Public quick check)")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        // ── Public (for frontend validation) ──
        group.MapPost("/validate", ValidateCouponForUserAsync)
            .WithName("ValidateCouponForUser")
            .WithDescription("Validate coupon cho người dùng với danh sách khóa học (Public)")
            .AllowAnonymous()
            .Produces<ApiResponse<CouponValidationResult>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CouponValidationResult>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetUserUsageHistoryAsync(
        [AsParameters] PaginationRequest pagination,
        [FromServices] ICouponUsageService couponUsageService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await couponUsageService.GetUserUsageHistoryAsync(currentUserService.UserId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CanUserUseCouponAsync(
        [FromQuery] Guid userId,
        [FromQuery] string couponCode,
        [FromServices] ICouponUsageService couponUsageService)
    {
        var result = await couponUsageService.CanUserUseCouponAsync(userId, couponCode);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ValidateCouponForUserAsync(
        [FromBody] ValidateCouponRequest request,
        [FromServices] ICouponUsageService couponUsageService)
    {
        var result = await couponUsageService.ValidateCouponAsync(
            request.CouponCode,
            request.UserId,
            request.CourseIds,
            request.OrderSubtotal);

        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}

// Request DTO for coupon validation
public class ValidateCouponRequest
{
    public string CouponCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public List<Guid> CourseIds { get; set; } = new();
    public decimal OrderSubtotal { get; set; }
}