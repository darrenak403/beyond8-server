using Beyond8.Assessment.Application.Dtos.QuizAttempts;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis;

public static class QuizAttemptApis
{
    public static IEndpointRouteBuilder MapQuizAttemptApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/quiz-attempts")
            .MapQuizAttemptRoutes()
            .WithTags("Quiz Attempt Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapQuizAttemptRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/start/{quizId:guid}", StartQuizAttemptAsync)
            .WithName("StartQuizAttempt")
            .WithDescription("Bắt đầu làm quiz - tạo quiz attempt mới cho student")
            .RequireAuthorization()
            .Produces<ApiResponse<StartQuizResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<StartQuizResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/{attemptId:guid}/submit", SubmitQuizAttemptAsync)
            .WithName("SubmitQuizAttempt")
            .WithDescription("Nộp bài làm quiz - chấm điểm và trả về kết quả")
            .RequireAuthorization()
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{attemptId:guid}/result", GetQuizAttemptResultAsync)
            .WithName("GetQuizAttemptResult")
            .WithDescription("Xem kết quả bài làm quiz đã nộp")
            .RequireAuthorization()
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<QuizResultResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> StartQuizAttemptAsync(
        [FromRoute] Guid quizId,
        [FromServices] IQuizAttemptService quizAttemptService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await quizAttemptService.CreateQuizAttemptAsync(quizId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> SubmitQuizAttemptAsync(
        [FromRoute] Guid attemptId,
        [FromBody] SubmitQuizRequest request,
        [FromServices] IQuizAttemptService quizAttemptService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await quizAttemptService.SubmitQuizAttemptAsync(attemptId, request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetQuizAttemptResultAsync(
        [FromRoute] Guid attemptId,
        [FromServices] IQuizAttemptService quizAttemptService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await quizAttemptService.GetQuizAttemptResultAsync(attemptId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
