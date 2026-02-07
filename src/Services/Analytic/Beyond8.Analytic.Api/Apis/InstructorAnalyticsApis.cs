using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.InstructorRevenue;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using static Beyond8.Common.Utilities.Const;

namespace Beyond8.Analytic.Api.Apis;

public static class InstructorAnalyticsApis
{
    public static IEndpointRouteBuilder MapInstructorAnalyticsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/analytics/instructors")
            .MapInstructorAnalyticsRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Instructor Revenue Analytics");

        return builder;
    }

    private static RouteGroupBuilder MapInstructorAnalyticsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllInstructorRevenueAsync)
            .WithName("GetAllInstructorRevenue")
            .WithDescription("Lấy thống kê doanh thu tất cả giảng viên (phân trang, lọc theo ngày)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<InstructorRevenueResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<InstructorRevenueResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/me", GetMyRevenueAsync)
            .WithName("GetMyRevenue")
            .WithDescription("Lấy thống kê doanh thu của giảng viên hiện tại")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<InstructorRevenueResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<InstructorRevenueResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/top", GetTopInstructorsAsync)
            .WithName("GetTopInstructors")
            .WithDescription("Lấy danh sách giảng viên hàng đầu (theo doanh thu, học viên, đánh giá)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<TopInstructorResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<TopInstructorResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetAllInstructorRevenueAsync(
        [AsParameters] DateRangeAnalyticRequest request,
        [FromServices] IInstructorRevenueService service)
    {
        var result = await service.GetAllInstructorRevenueAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyRevenueAsync(
        [FromServices] IInstructorRevenueService service,
        [FromServices] ICurrentUserService currentUserService)
    {
        var instructorId = currentUserService.UserId;
        var result = await service.GetInstructorRevenueAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetTopInstructorsAsync(
        [FromQuery] int count,
        [FromQuery] string sortBy,
        [FromServices] IInstructorRevenueService service)
    {
        var result = await service.GetTopInstructorsAsync(count <= 0 ? 10 : count, string.IsNullOrEmpty(sortBy) ? "revenue" : sortBy);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
