using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

public static class LessonApis
{
    public static IEndpointRouteBuilder MapLessonApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/lessons")
            .MapLessonRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Lesson Api");

        return builder;
    }

    public static RouteGroupBuilder MapLessonRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/video/callback", CallbackHlsAsync)
            .WithName("CallbackHls")
            .WithDescription("Callback để xử lý video HLS")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // Get lessons by section
        group.MapGet("/section/{sectionId}", GetLessonsBySectionIdAsync)
            .WithName("GetLessonsBySectionId")
            .WithDescription("Lấy danh sách bài học của chương")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<LessonResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<LessonResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get lesson by id
        group.MapGet("/{id}", GetLessonByIdAsync)
            .WithName("GetLessonById")
            .WithDescription("Lấy thông tin bài học theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create lesson
        group.MapPost("/", CreateLessonAsync)
            .WithName("CreateLesson")
            .WithDescription("Tạo bài học mới")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update lesson
        group.MapPatch("/{id}", UpdateLessonAsync)
            .WithName("UpdateLesson")
            .WithDescription("Cập nhật thông tin bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete lesson
        group.MapDelete("/{id}", DeleteLessonAsync)
            .WithName("DeleteLesson")
            .WithDescription("Xóa bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }
    private static async Task<IResult> CallbackHlsAsync(VideoCallbackDto request, ILessonService lessonService)
    {
        var response = await lessonService.CallbackHlsAsync(request);
        return Results.Ok(response);
    }

    private static async Task<IResult> GetLessonsBySectionIdAsync(
        Guid sectionId,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonService.GetLessonsBySectionIdAsync(sectionId, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetLessonByIdAsync(
        Guid id,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonService.GetLessonByIdAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateLessonAsync(
        [FromBody] CreateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.CreateLessonAsync(request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> UpdateLessonAsync(
        Guid id,
        [FromBody] UpdateLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.UpdateLessonAsync(id, request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> DeleteLessonAsync(
        Guid id,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonService.DeleteLessonAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
