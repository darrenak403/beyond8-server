using Beyond8.Learning.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Beyond8.Learning.Api.Apis;

public static class InternalLearningApis
{
    public static IEndpointRouteBuilder MapInternalLearningApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/internal/stats")
            .WithTags("Internal - Analytics")
            .AllowAnonymous();

        group.MapGet("/enrollments", GetPlatformEnrollmentStats)
            .WithName("GetPlatformEnrollmentStats")
            .WithSummary("Get platform enrollment counts for analytics");

        return app;
    }

    private static async Task<IResult> GetPlatformEnrollmentStats(
        [FromServices] IEnrollmentService enrollmentService)
    {
        var result = await enrollmentService.GetPlatformEnrollmentStatsAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(500);
    }
}
