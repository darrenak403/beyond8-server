using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Clients;
using Beyond8.Integration.Application.Dtos.AiIntegration.Profile;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
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

            group.MapPost("/lesson/quiz/generate", GenerateQuiz)
                .WithName("GenerateQuiz")
                .WithDescription("Sinh quiz từ ngữ cảnh khóa học. Chia 3 cấp độ Easy/Medium/Hard, số lượng theo request.")
                .RequireAuthorization()
                .Produces<ApiResponse<GenQuizResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<GenQuizResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/quiz/format/questions", FormatQuizQuestionsFromPdf)
                .WithName("FormatQuizQuestionsFromPdf")
                .WithDescription("Format câu hỏi quiz từ file PDF. Trích text từ PDF, gửi AI đọc và trả về danh sách câu hỏi cấu trúc.")
                .RequireAuthorization()
                .DisableAntiforgery()
                .Produces<ApiResponse<List<GenQuizResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<GenQuizResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            // group.MapPost("/quiz/explain", ExplainQuiz)
            //     .WithName("ExplainQuiz")
            //     .WithDescription("Giải thích quiz từ câu hỏi và đáp án cho sinh viên.")
            //     .RequireAuthorization()
            //     .Produces<ApiResponse<ExplainQuizResponse>>(StatusCodes.Status200OK)
            //     .Produces<ApiResponse<ExplainQuizResponse>>(StatusCodes.Status400BadRequest)
            //     .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/health", HealthCheck)
                .WithName("HealthCheck")
                .WithDescription("Check the health of the AI service (generative AI)")
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/embed", EmbedCourseDocuments)
                .WithName("EmbedCourseDocuments")
                .WithDescription("Upload PDF, chunk và embed vào Qdrant (Instructor only)")
                .DisableAntiforgery()
                .RequireAuthorization(r => r.RequireRole(Role.Instructor))
                .Produces<ApiResponse<EmbedCourseDocumentsResult>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<EmbedCourseDocumentsResult>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/embed/health", EmbeddingHealthCheck)
                .WithName("EmbeddingHealthCheck")
                .WithDescription("Check the health of the embedding service (Qdrant, Hugging Face)")
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

            return group;
        }

        private static async Task<IResult> FormatQuizQuestionsFromPdf(
            [FromForm] IFormFile file,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient,
            [FromServices] IAiService aiService)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
                return Results.BadRequest(ApiResponse<List<GenQuizResponse>>.FailureResponse(check.Message, check.Metadata));

            if (file == null || file.Length == 0)
                return Results.BadRequest(ApiResponse<List<GenQuizResponse>>.FailureResponse("File không được để trống."));

            if (file.ContentType != "application/pdf")
                return Results.BadRequest(ApiResponse<List<GenQuizResponse>>.FailureResponse("Chỉ chấp nhận file PDF."));

            await using var stream = file.OpenReadStream();
            var result = await aiService.FormatQuizQuestionsFromPdfAsync(stream, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> InstructorProfileReview(
            [FromBody] ProfileReviewRequest request,
            [FromServices] IAiService aiService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient,
            [FromServices] IValidator<ProfileReviewRequest> validator)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
                return Results.BadRequest(ApiResponse<AiProfileReviewResponse>.FailureResponse(check.Message, check.Metadata));

            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await aiService.InstructorProfileReviewAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GenerateQuiz(
            [FromBody] GenQuizRequest request,
            [FromServices] IAiService aiService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient,
            [FromServices] IValidator<GenQuizRequest> validator)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
                return Results.BadRequest(ApiResponse<GenQuizResponse>.FailureResponse(check.Message, check.Metadata));

            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await aiService.GenerateQuizAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> EmbedCourseDocuments(
            [FromForm] EmbedCourseDocumentsRequest request,
            [FromForm] IFormFile file,
            [FromServices] IEmbeddingService embeddingService,
            [FromServices] IValidator<EmbedCourseDocumentsRequest> validator,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IIdentityClient identityClient)
        {
            var check = await SubscriptionHelper.CheckSubscriptionStatusAsync(identityClient, currentUserService.UserId);
            if (!check.IsAllowed)
                return Results.BadRequest(ApiResponse<EmbedCourseDocumentsResult>.FailureResponse(check.Message, check.Metadata));

            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            if (file == null || file.Length == 0)
                return Results.BadRequest(ApiResponse<EmbedCourseDocumentsResult>.FailureResponse("File không được để trống."));

            if (file.ContentType != "application/pdf")
                return Results.BadRequest(ApiResponse<EmbedCourseDocumentsResult>.FailureResponse("Chỉ chấp nhận file PDF."));

            await using var stream = file.OpenReadStream();
            var result = await embeddingService.EmbedCourseDocumentsAsync(stream, request);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> EmbeddingHealthCheck([FromServices] IEmbeddingService embeddingService)
        {
            var result = await embeddingService.CheckHealthAsync();
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> HealthCheck(
            [FromServices] IIdentityClient identityClient,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IGenerativeAiService aiService
        )
        {
            var result = await aiService.CheckHealthAsync();
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
