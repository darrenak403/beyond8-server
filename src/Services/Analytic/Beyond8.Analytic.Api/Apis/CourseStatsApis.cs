using Beyond8.Analytic.Application.Dtos.Common;
using Beyond8.Analytic.Application.Dtos.CourseStats;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;
using static Beyond8.Common.Utilities.Const;

namespace Beyond8.Analytic.Api.Apis;

public static class CourseStatsApis
{
    public static IEndpointRouteBuilder MapCourseStatsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/analytics/courses")
            .MapCourseStatsRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Course Stats Analytics");

        return builder;
    }

    private static RouteGroupBuilder MapCourseStatsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetAllCourseStatsAsync)
            .WithName("GetAllCourseStats")
            .WithDescription("Lấy thống kê tất cả khóa học (phân trang, lọc theo ngày)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<CourseStatsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseStatsResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/top", GetTopCoursesAsync)
            .WithName("GetTopCourses")
            .WithDescription("Lấy danh sách khóa học hàng đầu (theo học viên, doanh thu, đánh giá)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<TopCourseResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<TopCourseResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/instructor", GetInstructorCourseStatsAsync)
            .WithName("GetInstructorCourseStats")
            .WithDescription("Lấy thống kê khóa học của giảng viên hiện tại")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<CourseStatsResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseStatsResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{courseId:guid}", GetCourseStatsByIdAsync)
            .WithName("GetCourseStatsById")
            .WithDescription("Lấy thống kê khóa học theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff, Role.Instructor))
            .Produces<ApiResponse<CourseStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseStatsResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetAllCourseStatsAsync(
        [AsParameters] DateRangeAnalyticRequest request,
        [FromServices] ICourseStatsService service)
    {
        var result = await service.GetAllCourseStatsAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseStatsByIdAsync(
        [FromRoute] Guid courseId,
        [FromServices] ICourseStatsService service)
    {
        var result = await service.GetCourseStatsByIdAsync(courseId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetTopCoursesAsync(
        [FromQuery] int count,
        [FromQuery] string sortBy,
        [FromServices] ICourseStatsService service)
    {
        var result = await service.GetTopCoursesAsync(count <= 0 ? 10 : count, string.IsNullOrEmpty(sortBy) ? "students" : sortBy);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetInstructorCourseStatsAsync(
        [AsParameters] PaginationRequest pagination,
        [FromServices] ICourseStatsService service,
        [FromServices] ICurrentUserService currentUserService)
    {
        var instructorId = currentUserService.UserId;
        var result = await service.GetInstructorCourseStatsAsync(instructorId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
