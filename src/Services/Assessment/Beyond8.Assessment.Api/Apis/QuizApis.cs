using Beyond8.Assessment.Application.Dtos.Quizzes;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis
{
    public static class QuizApis
    {
        public static IEndpointRouteBuilder MapQuizApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/quizzes")
                .MapQuizRoutes()
                .WithTags("Quiz Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapQuizRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateQuizAsync)
                .WithName("CreateQuiz")
                .WithDescription("Tạo quiz mới (nhận danh sách QuestionId)")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<QuizSimpleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<QuizSimpleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id}", GetQuizByIdAsync)
                .WithName("GetQuizById")
                .WithDescription("Lấy quiz theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id}", UpdateQuizAsync)
                .WithName("UpdateQuiz")
                .WithDescription("Cập nhật quiz")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<QuizResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id}", DeleteQuizAsync)
                .WithName("DeleteQuiz")
                .WithDescription("Xóa quiz")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> DeleteQuizAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromRoute] Guid id,
            [FromServices] IQuizService quizService)
        {
            var result = await quizService.DeleteQuizAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> UpdateQuizAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromRoute] Guid id,
            [FromBody] UpdateQuizRequest request,
            [FromServices] IQuizService quizService,
            [FromServices] IValidator<UpdateQuizRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await quizService.UpdateQuizAsync(id, request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> GetQuizByIdAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromRoute] Guid id,
            [FromServices] IQuizService quizService)
        {
            var result = await quizService.GetQuizByIdAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> CreateQuizAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] CreateQuizRequest request,
            [FromServices] IQuizService quizService,
            [FromServices] IValidator<CreateQuizRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await quizService.CreateQuizAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}