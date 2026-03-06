using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Application.Services.Interfaces;
using Beyond8.Common.Utilities;
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

        return group;
    }

    private static async Task<IResult> GetSystemDashboardAsync(
        [FromServices] ISystemOverviewService service)
    {
        var result = await service.GetSystemDashboardAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
