using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration.Embedding;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Application.Validators.AiIntegration;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis;

public static class EmbeddingApis
{
    public static IEndpointRouteBuilder MapEmbeddingApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/embeddings")
            .MapEmbeddingRoutes()
            .WithTags("Embedding Api")
            .RequireAuthorization();

        return builder;
    }

    private static RouteGroupBuilder MapEmbeddingRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/", EmbedCourseDocuments)
            .WithName("EmbedCourseDocuments")
            .WithDescription("Upload PDF file và tự động chunk, embed vào Qdrant")
            .DisableAntiforgery()
            .Produces<ApiResponse<List<DocumentEmbeddingResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<DocumentEmbeddingResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> EmbedCourseDocuments(
        [FromForm] EmbedCourseDocumentsRequest request,
        [FromForm] IFormFile file,
        [FromServices] IVectorEmbeddingService embeddingService,
        [FromServices] IValidator<EmbedCourseDocumentsRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        if (file == null || file.Length == 0)
        {
            return Results.BadRequest(ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("File không được để trống."));
        }

        if (file.ContentType != "application/pdf")
        {
            return Results.BadRequest(ApiResponse<List<DocumentEmbeddingResponse>>.FailureResponse("Chỉ chấp nhận file PDF."));
        }

        await using var stream = file.OpenReadStream();
        var result = await embeddingService.EmbedAndSavePdfAsync(
            stream,
            request.CourseId,
            request.DocumentId,
            request.LessonId);

        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
}
