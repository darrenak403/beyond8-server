using Beyond8.Catalog.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Beyond8.Catalog.Api.Apis;

public static class InternalCatalogApis
{
    public static IEndpointRouteBuilder MapInternalCatalogApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/internal/stats")
            .WithTags("Internal - Analytics")
            .AllowAnonymous();

        group.MapGet("/courses", GetPlatformCourseStats)
            .WithName("GetPlatformCourseStats")
            .WithSummary("Get platform course counts for analytics");

        return app;
    }

    private static async Task<IResult> GetPlatformCourseStats(
        [FromServices] ICourseService courseService)
    {
        var result = await courseService.GetPlatformCourseStatsAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(500);
    }
}
