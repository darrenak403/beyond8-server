using Beyond8.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Beyond8.Identity.Api.Apis;

public static class InternalIdentityApis
{
    public static IEndpointRouteBuilder MapInternalIdentityApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/internal/stats")
            .WithTags("Internal - Analytics")
            .AllowAnonymous();

        group.MapGet("/users", GetPlatformUserStats)
            .WithName("GetPlatformUserStats")
            .WithSummary("Get platform user counts for analytics");

        return app;
    }

    private static async Task<IResult> GetPlatformUserStats(
        [FromServices] IUserService userService)
    {
        var result = await userService.GetPlatformUserStatsAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(500);
    }
}
