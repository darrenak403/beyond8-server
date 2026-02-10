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
        group.MapGet("/overview", GetSystemOverviewAsync)
            .WithName("GetSystemOverview")
            .WithDescription("Lấy tổng quan hệ thống")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<SystemOverviewResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SystemOverviewResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetSystemOverviewAsync(
        [FromServices] ISystemOverviewService service)
    {
        var result = await service.GetSystemOverviewAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
