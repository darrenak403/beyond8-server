using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Ai;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis;

public static class AiApis
{
    public static IEndpointRouteBuilder MapAiApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/ai")
            .MapAiRoutes()
            .WithTags("AI Api")
            .RequireRateLimiting("AiFixedLimit")
            .RequireAuthorization();

        return builder;
    }

    private static RouteGroupBuilder MapAiRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/profile-review", InstructorProfileReview)
            .WithName("InstructorProfileReview")
            .WithDescription("Review instructor profile by AI (Require Authorization)")
            .RequireAuthorization()
            .Produces<ApiResponse<AiProfileReviewResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AiProfileReviewResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> InstructorProfileReview(
        [FromBody] ProfileReviewRequest request,
        [FromServices] IAiService aiService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await aiService.InstructorProfileReviewAsync(request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
