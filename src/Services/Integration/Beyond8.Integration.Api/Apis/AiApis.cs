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
        group.MapPost("/instructor-application-review", InstructorApplicationReview)
            .WithName("InstructorApplicationReview")
            .WithDescription("Review instructor application")
            .Produces<ApiResponse<AiInstructorApplicationReviewResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<AiInstructorApplicationReviewResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> InstructorApplicationReview(
        [FromBody] CreateInstructorProfileRequest request,
        [FromServices] IAiService aiService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await aiService.InstructorApplicationReviewAsync(request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
