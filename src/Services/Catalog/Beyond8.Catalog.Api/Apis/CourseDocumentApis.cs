using Beyond8.Catalog.Application.Dtos.CourseDocuments;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

public static class CourseDocumentApis
{
    public static IEndpointRouteBuilder MapCourseDocumentApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/course-documents")
            .MapCourseDocumentRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Course Document Api");

        return builder;
    }

    public static RouteGroupBuilder MapCourseDocumentRoutes(this RouteGroupBuilder group)
    {
        // Get documents by course
        group.MapGet("/course/{courseId}", GetCourseDocumentsAsync)
            .WithName("GetCourseDocuments")
            .WithDescription("Lấy danh sách tài liệu của khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<CourseDocumentResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseDocumentResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get document by id
        group.MapGet("/{id}", GetCourseDocumentByIdAsync)
            .WithName("GetCourseDocumentById")
            .WithDescription("Lấy thông tin tài liệu theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create document
        group.MapPost("/", CreateCourseDocumentAsync)
            .WithName("CreateCourseDocument")
            .WithDescription("Tạo tài liệu mới cho khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update document
        group.MapPatch("/{id}", UpdateCourseDocumentAsync)
            .WithName("UpdateCourseDocument")
            .WithDescription("Cập nhật thông tin tài liệu")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseDocumentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete document
        group.MapDelete("/{id}", DeleteCourseDocumentAsync)
            .WithName("DeleteCourseDocument")
            .WithDescription("Xóa tài liệu")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Toggle downloadable
        group.MapPatch("/{id}/toggle-downloadable", ToggleDownloadableAsync)
            .WithName("ToggleDocumentDownloadable")
            .WithDescription("Thay đổi trạng thái cho phép tải xuống")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Increment download count (public endpoint for download tracking)
        group.MapPost("/{id}/download", IncrementDownloadCountAsync)
            .WithName("IncrementDocumentDownloadCount")
            .WithDescription("Tăng số lượt tải xuống tài liệu")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // Update vector index status
        group.MapPatch("/{id}/vector-index", UpdateVectorIndexStatusAsync)
            .WithName("UpdateDocumentVectorIndexStatus")
            .WithDescription("Cập nhật trạng thái vector index")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetCourseDocumentsAsync(
        [FromRoute] Guid courseId,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.GetCourseDocumentsAsync(courseId, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseDocumentByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.GetCourseDocumentByIdAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateCourseDocumentAsync(
        [FromBody] CreateCourseDocumentRequest request,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] IValidator<CreateCourseDocumentRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.CreateCourseDocumentAsync(request, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateCourseDocumentAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateCourseDocumentRequest request,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] IValidator<UpdateCourseDocumentRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.UpdateCourseDocumentAsync(id, request, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteCourseDocumentAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.DeleteCourseDocumentAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ToggleDownloadableAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.ToggleDownloadableAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> IncrementDownloadCountAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseDocumentService courseDocumentService)
    {
        var result = await courseDocumentService.IncrementDownloadCountAsync(id);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateVectorIndexStatusAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateVectorIndexRequest request,
        [FromServices] ICourseDocumentService courseDocumentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseDocumentService.UpdateVectorIndexStatusAsync(id, request.IsIndexed, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}