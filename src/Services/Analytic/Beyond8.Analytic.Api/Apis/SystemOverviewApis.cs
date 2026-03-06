using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Analytic.Api.Apis;

public static class SystemOverviewApis
{
    public static IEndpointRouteBuilder MapSystemOverviewApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/analytics/system")
            .MapSystemOverviewRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("System Overview Analytics");

        return builder;
    }

    private static RouteGroupBuilder MapSystemOverviewRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/dashboard", GetSystemDashboardAsync)
            .WithName("GetSystemDashboard")
            .WithDescription("Lấy dashboard tổng quan hệ thống (KPI cards + xu hướng 12 tháng)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<SystemDashboardResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SystemDashboardResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/revenue-trend", GetRevenueTrendAsync)
            .WithName("GetRevenueTrend")
            .WithDescription("Thống kê doanh thu theo năm / quý / tháng / khoảng thời gian")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<RevenueTrendResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<RevenueTrendResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/backfill-revenue", BackfillRevenueAsync)
            .WithName("BackfillRevenue")
            .WithDescription("Backfill dữ liệu doanh thu lịch sử từ Sale Service vào Analytic aggregates (Admin only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<BackfillRevenueResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<BackfillRevenueResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetSystemDashboardAsync(
        [FromServices] ISystemOverviewService service)
    {
        var result = await service.GetSystemDashboardAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetRevenueTrendAsync(
        [AsParameters] RevenueTrendRequest request,
        [FromServices] ISystemOverviewService service,
        [FromServices] IValidator<RevenueTrendRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await service.GetRevenueTrendAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> BackfillRevenueAsync(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromServices] ISystemOverviewService service)
    {
        if (from > to)
            return Results.BadRequest(ApiResponse<BackfillRevenueResponse>.FailureResponse("'from' phải nhỏ hơn hoặc bằng 'to'"));

        if ((to - from).TotalDays > 366)
            return Results.BadRequest(ApiResponse<BackfillRevenueResponse>.FailureResponse("Khoảng thời gian tối đa là 366 ngày"));

        var result = await service.BackfillRevenueAsync(from, to);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
