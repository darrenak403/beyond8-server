using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Dtos.Ai;
using Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;
using Beyond8.Integration.Application.Helpers.AiService;
using Beyond8.Integration.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis
{
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

            group.MapPost("/quiz/generate", GenerateQuiz)
                .WithName("GenerateQuiz")
                .WithDescription("Sinh quiz từ ngữ cảnh khóa học. Chia 3 cấp độ Easy/Medium/Hard, số lượng theo request.")
                .RequireAuthorization()
                .Produces<ApiResponse<GenQuizResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<GenQuizResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/health", HealthCheck)
                .WithName("HealthCheck")
                .WithDescription("Check the health of the AI service")
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> HealthCheck(
            [FromServices] IIdentityClient identityClient,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IGenerativeAiService aiService
        )
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
            {
                return Results.BadRequest(ApiResponse<bool>.FailureResponse(check.Message, check.Metadata));
            }
            var result = await aiService.CheckHealthAsync();
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> InstructorProfileReview(
            [FromBody] ProfileReviewRequest request,
            [FromServices] IAiService aiService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
            {
                return Results.BadRequest(ApiResponse<AiProfileReviewResponse>.FailureResponse(check.Message, check.Metadata));
            }
            var result = await aiService.InstructorProfileReviewAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GenerateQuiz(
            [FromBody] GenQuizRequest request,
            [FromServices] IAiService aiService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
            {
                return Results.BadRequest(ApiResponse<GenQuizResponse>.FailureResponse(check.Message, check.Metadata));
            }
            var result = await aiService.GenerateQuizAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
