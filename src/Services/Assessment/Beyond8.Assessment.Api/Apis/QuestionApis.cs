using Beyond8.Assessment.Application.Dtos.Questions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
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
            group.MapGet("/", GetQuestionsAsync)
                .WithName("GetQuestions")
                .WithDescription("Lấy danh sách câu hỏi (có thể lọc theo tag).")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<QuestionResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<QuestionResponse>>>(StatusCodes.Status400BadRequest);

            group.MapGet("/tags/count", GetTagCountsAsync)
                .WithName("GetQuestionTagCounts")
                .WithDescription("Lấy số lượng câu hỏi theo từng thẻ (tag).")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<TagCountResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<TagCountResponse>>>(StatusCodes.Status400BadRequest);

            group.MapPost("/import-from-ai", ImportQuestionsFromAiAsync)
                .WithName("ImportQuestionsFromAi")
                .WithDescription("Nhập câu hỏi từ AI")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status400BadRequest);

            group.MapPost("/bulk", CreateQuestionsAsync)
                .WithName("CreateQuestion")
                .WithDescription("Tạo câu hỏi")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<QuestionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<QuestionResponse>>(StatusCodes.Status400BadRequest);

            group.MapPost("/", CreateQuestionAsync)
                .WithName("CreateQuestion")
                .WithDescription("Tạo câu hỏi")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<QuestionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<QuestionResponse>>(StatusCodes.Status400BadRequest);

            group.MapPatch("/{id}", UpdateQuestionAsync)
                .WithName("UpdateQuestion")
                .WithDescription("Cập nhật câu hỏi")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            group.MapDelete("/{id}", DeleteQuestionAsync)
                .WithName("DeleteQuestion")
                .WithDescription("Xóa câu hỏi")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> GetQuestionsAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IQuestionService questionService,
            [FromServices] IValidator<GetQuestionsRequest> validator,
            [AsParameters] GetQuestionsRequest request)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await questionService.GetQuestionsAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetTagCountsAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IQuestionService questionService)
        {
            var result = await questionService.GetTagCountsAsync(currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteQuestionAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromRoute] Guid id,
            [FromServices] IQuestionService questionService)
        {
            var result = await questionService.DeleteQuestionAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateQuestionAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromRoute] Guid id,
            [FromBody] QuestionRequest request,
            [FromServices] IQuestionService questionService,
            [FromServices] IValidator<QuestionRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await questionService.UpdateQuestionAsync(id, request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> CreateQuestionAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] QuestionRequest request,
            [FromServices] IQuestionService questionService,
            [FromServices] IValidator<QuestionRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await questionService.CreateQuestionAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> CreateQuestionsAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] List<QuestionRequest> requests,
            [FromServices] IQuestionService questionService,
            [FromServices] IValidator<QuestionRequest> validator)
        {
            if (!requests.Select(x => x.ValidateRequest(validator, out var validationResult)).All(x => x))
                return Results.BadRequest(ApiResponse<List<QuestionResponse>>.FailureResponse("Vui lòng kiểm tra lại dữ liệu đầu vào"));

            var result = await questionService.CreateQuestionsAsync(requests, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> ImportQuestionsFromAiAsync(
            [FromServices] ICurrentUserService currentUserService,
            [FromBody] QuestionFromAiRequest request,
            [FromServices] IQuestionService questionService,
            [FromServices] IValidator<QuestionFromAiRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await questionService.ImportQuestionsFromAiAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}