using Beyond8.Catalog.Application.Dtos.LessonDocuments;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

public static class LessonDocumentApis
{
    public static IEndpointRouteBuilder MapLessonDocumentApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/lesson-documents")
            .MapLessonDocumentRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Lesson Document Api");

        return builder;
    }

    public static RouteGroupBuilder MapLessonDocumentRoutes(this RouteGroupBuilder group)
    {
        // Get documents by lesson
        group.MapGet("/lesson/{lessonId}", GetLessonDocumentsAsync)
            .WithName("GetLessonDocuments")
            .WithDescription("Lấy danh sách tài liệu của bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/lesson/{lessonId}/student", GetLessonDocumentsForStudentAsync)
            .WithName("GetLessonDocumentsForStudent")
            .WithDescription("Lấy danh sách tài liệu của bài học cho học viên")
            .RequireAuthorization(x => x.RequireRole(Role.Student))
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get document by id
        group.MapGet("/{id}", GetLessonDocumentByIdAsync)
            .WithName("GetLessonDocumentById")
            .WithDescription("Lấy thông tin tài liệu bài học theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create document
        group.MapPost("/", CreateLessonDocumentAsync)
            .WithName("CreateLessonDocument")
            .WithDescription("Tạo tài liệu mới cho bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update document
        group.MapPatch("/{id}", UpdateLessonDocumentAsync)
            .WithName("UpdateLessonDocument")
            .WithDescription("Cập nhật thông tin tài liệu bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete document
        group.MapDelete("/{id}", DeleteLessonDocumentAsync)
            .WithName("DeleteLessonDocument")
            .WithDescription("Xóa tài liệu bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Toggle downloadable
        group.MapPatch("/{id}/toggle-downloadable", ToggleDownloadableAsync)
            .WithName("ToggleLessonDocumentDownloadable")
            .WithDescription("Thay đổi trạng thái cho phép tải xuống tài liệu bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Increment download count (public endpoint for download tracking)
        group.MapPost("/{id}/download", IncrementDownloadCountAsync)
            .WithName("IncrementLessonDocumentDownloadCount")
            .WithDescription("Tăng số lượt tải xuống tài liệu bài học")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // Update vector index status
        group.MapPatch("/{id}/vector-index", UpdateVectorIndexStatusAsync)
            .WithName("UpdateLessonDocumentVectorIndex")
            .WithDescription("Cập nhật trạng thái vector index của tài liệu bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get preview documents by lesson (public endpoint for course discovery)
        group.MapGet("/lesson/{lessonId}/preview", GetLessonDocumentsPreviewAsync)
            .WithName("GetLessonDocumentsPreview")
            .WithDescription("Lấy danh sách tài liệu preview của bài học (công khai)")
            .AllowAnonymous()
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<LessonDocumentResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetLessonDocumentsAsync(
        [FromRoute] Guid lessonId,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.GetLessonDocumentsAsync(lessonId, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetLessonDocumentsForStudentAsync(
    [FromRoute] Guid lessonId,
    [FromServices] ILessonDocumentService lessonDocumentService,
    [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.GetLessonDocumentsForStudentAsync(lessonId, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetLessonDocumentByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.GetLessonDocumentByIdAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateLessonDocumentAsync(
        [FromBody] CreateLessonDocumentRequest request,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] IValidator<CreateLessonDocumentRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.CreateLessonDocumentAsync(request, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateLessonDocumentAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateLessonDocumentRequest request,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] IValidator<UpdateLessonDocumentRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.UpdateLessonDocumentAsync(id, request, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteLessonDocumentAsync(
        [FromRoute] Guid id,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.DeleteLessonDocumentAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ToggleDownloadableAsync(
        [FromRoute] Guid id,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.ToggleDownloadableAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> IncrementDownloadCountAsync(
        [FromRoute] Guid id,
        [FromServices] ILessonDocumentService lessonDocumentService)
    {
        var result = await lessonDocumentService.IncrementDownloadCountAsync(id);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateVectorIndexStatusAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateVectorIndexRequest request,
        [FromServices] ILessonDocumentService lessonDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonDocumentService.UpdateVectorIndexStatusAsync(id, request.IsIndexed, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetLessonDocumentsPreviewAsync(
        [FromRoute] Guid lessonId,
        [FromServices] ILessonDocumentService lessonDocumentService)
    {
        var result = await lessonDocumentService.GetLessonDocumentsPreviewAsync(lessonId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
