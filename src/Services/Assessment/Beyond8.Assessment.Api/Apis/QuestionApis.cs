using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis
{
    public static class QuestionApis
    {
        public static IEndpointRouteBuilder MapQuestionApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/questions")
                .MapQuestionRoutes()
                .WithTags("Question Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapQuestionRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/import-from-ai", ImportQuestionsFromAiAsync)
                .WithName("ImportQuestionsFromAi")
                .WithDescription("Nhập câu hỏi từ AI")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> ImportQuestionsFromAiAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] QuestionFromAiRequest request,
            [FromServices] IQuestionService questionService)
        {
            var result = await questionService.ImportQuestionsFromAiAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}