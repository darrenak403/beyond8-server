using Beyond8.Analytic.Application.Dtos.LessonPerformance;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Analytic.Api.Apis;

public static class LessonPerformanceApis
{
    public static IEndpointRouteBuilder MapLessonPerformanceApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/analytics/lessons")
            .MapLessonPerformanceRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Lesson Performance Analytics");

        return builder;
    }

    private static RouteGroupBuilder MapLessonPerformanceRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/by-course/{courseId:guid}", GetLessonPerformanceByCourseAsync)
            .WithName("GetLessonPerformanceByCourse")
            .WithDescription("Lấy thống kê hiệu suất bài học theo khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff, Role.Instructor))
            .Produces<ApiResponse<List<LessonPerformanceResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<LessonPerformanceResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetLessonPerformanceByCourseAsync(
        [FromRoute] Guid courseId,
        [FromServices] ILessonPerformanceService service)
    {
        var result = await service.GetLessonPerformanceByCourseAsync(courseId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
