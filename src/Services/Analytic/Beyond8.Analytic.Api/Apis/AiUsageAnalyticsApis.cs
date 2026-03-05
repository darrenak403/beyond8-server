using Beyond8.Analytic.Application.Dtos.AiUsage;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Analytic.Api.Apis;

public static class AiUsageAnalyticsApis
{
    public static IEndpointRouteBuilder MapAiUsageAnalyticsApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/analytics/ai-usage")
            .MapAiUsageAnalyticsRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("AI Usage Analytics");

        return builder;
    }

    private static RouteGroupBuilder MapAiUsageAnalyticsRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/chart", GetAiUsageChartAsync)
            .WithName("GetAiUsageChart")
            .WithDescription("Lấy dữ liệu AI usage theo khoảng thời gian để vẽ biểu đồ (1/3/6/9/12 tháng hoặc startDate-endDate)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<AiUsageDailyChartItemResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<AiUsageDailyChartItemResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetAiUsageChartAsync(
        [AsParameters] AiUsageChartRequest request,
        [FromServices] IAiUsageAnalyticService service)
    {
        var result = await service.GetAiUsageChartAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
