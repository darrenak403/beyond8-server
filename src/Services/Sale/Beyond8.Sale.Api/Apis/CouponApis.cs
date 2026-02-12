using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Coupons;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class CouponApis
{
    public static IEndpointRouteBuilder MapCouponApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/coupons")
            .MapCouponRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Coupon Api");

        return builder;
    }

    public static RouteGroupBuilder MapCouponRoutes(this RouteGroupBuilder group)
    {
        // ── Admin ──
        group.MapPost("/admin", CreateAdminCouponAsync)
            .WithName("CreateAdminCoupon")
            .WithDescription("Tạo coupon cho toàn bộ khóa học (Admin only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/", GetCouponsAsync)
            .WithName("GetCoupons")
            .WithDescription("Lấy danh sách tất cả coupon (Admin only, paginated)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<List<CouponResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/{couponId}/toggle-status", ToggleCouponStatusAsync)
            .WithName("ToggleCouponStatus")
            .WithDescription("Bật/tắt trạng thái coupon (Admin only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // ── Instructor ──
        group.MapPost("/instructor", CreateInstructorCouponAsync)
            .WithName("CreateInstructorCoupon")
            .WithDescription("Tạo coupon cho khóa học cụ thể (Instructor only)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/instructor", GetCouponsByInstructorAsync)
            .WithName("GetCouponsByInstructor")
            .WithDescription("Lấy danh sách coupon của instructor (Instructor - own data, paginated)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<CouponResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // ── Admin / Instructor ──
        group.MapPut("/{couponId}", UpdateCouponAsync)
            .WithName("UpdateCoupon")
            .WithDescription("Cập nhật thông tin coupon (Admin or Instructor - owner only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Instructor))
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapDelete("/{couponId}", DeleteCouponAsync)
            .WithName("DeleteCoupon")
            .WithDescription("Xóa coupon (Admin or Instructor - owner only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // ── Public ──
        group.MapGet("/code/{code}", GetCouponByCodeAsync)
            .WithName("GetCouponByCode")
            .WithDescription("Lấy thông tin coupon theo mã (Public)")
            .AllowAnonymous()
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CouponResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("/active", GetActiveCouponsAsync)
            .WithName("GetActiveCoupons")
            .WithDescription("Lấy danh sách coupon đang hoạt động (Public)")
            .AllowAnonymous()
            .Produces<ApiResponse<List<CouponResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> CreateAdminCouponAsync(
        [FromBody] CreateAdminCouponRequest request,
        [FromServices] ICouponService couponService,
        [FromServices] IValidator<CreateAdminCouponRequest> validator)
    {
        // Validate request data
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await couponService.CreateAdminCouponAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateInstructorCouponAsync(
        [FromBody] CreateInstructorCouponRequest request,
        [FromServices] ICouponService couponService,
        [FromServices] IValidator<CreateInstructorCouponRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        // Validate request data
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await couponService.CreateInstructorCouponAsync(request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateCouponAsync(
        [FromRoute] Guid couponId,
        [FromBody] UpdateCouponRequest request,
        [FromServices] ICouponService couponService,
        [FromServices] IValidator<UpdateCouponRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        // Validate request data
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        // First get the coupon to check ownership
        var couponResult = await couponService.GetCouponByCodeAsync("temp"); // We need to get by ID, but service doesn't have GetById
        // For now, let the service handle authorization - this needs to be improved
        // TODO: Add GetCouponByIdAsync to service for proper authorization check

        var result = await couponService.UpdateCouponAsync(couponId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteCouponAsync(
        [FromRoute] Guid couponId,
        [FromServices] ICouponService couponService,
        [FromServices] ICurrentUserService currentUserService)
    {
        // For now, let the service handle authorization
        // TODO: Add GetCouponByIdAsync to service for proper authorization check

        var result = await couponService.DeleteCouponAsync(couponId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCouponsAsync(
        [AsParameters] PaginationRequest pagination,
        [FromServices] ICouponService couponService)
    {
        var result = await couponService.GetCouponsAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ToggleCouponStatusAsync(
        [FromRoute] Guid couponId,
        [FromServices] ICouponService couponService)
    {
        var result = await couponService.ToggleCouponStatusAsync(couponId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCouponsByInstructorAsync(
        [FromServices] ICouponService couponService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await couponService.GetCouponsByInstructorAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCouponByCodeAsync(
        [FromRoute] string code,
        [FromServices] ICouponService couponService)
    {
        var result = await couponService.GetCouponByCodeAsync(code);
        return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetActiveCouponsAsync(
        [FromServices] ICouponService couponService)
    {
        var result = await couponService.GetActiveCouponsAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}