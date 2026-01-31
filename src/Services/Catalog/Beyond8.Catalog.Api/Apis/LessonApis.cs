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

        // Delete lesson
        group.MapDelete("/{id}", DeleteLessonAsync)
            .WithName("DeleteLesson")
            .WithDescription("Xóa bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create video lesson
        group.MapPost("/video", CreateVideoLessonAsync)
            .WithName("CreateVideoLesson")
            .WithDescription("Tạo bài học video")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create text lesson
        group.MapPost("/text", CreateTextLessonAsync)
            .WithName("CreateTextLesson")
            .WithDescription("Tạo bài học văn bản")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create quiz lesson
        group.MapPost("/quiz", CreateQuizLessonAsync)
            .WithName("CreateQuizLesson")
            .WithDescription("Tạo bài học quiz")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update video lesson
        group.MapPatch("/{id}/video", UpdateVideoLessonAsync)
            .WithName("UpdateVideoLesson")
            .WithDescription("Cập nhật bài học video")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update text lesson
        group.MapPatch("/{id}/text", UpdateTextLessonAsync)
            .WithName("UpdateTextLesson")
            .WithDescription("Cập nhật bài học văn bản")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update quiz lesson
        group.MapPatch("/{id}/quiz", UpdateQuizLessonAsync)
            .WithName("UpdateQuizLesson")
            .WithDescription("Cập nhật bài học quiz")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update section assignment
        group.MapPatch("/{id}/change-quiz", ChangeQuizForLessonAsync)
            .WithName("ChangeQuizForLesson")
            .WithDescription("Thay đổi quiz khác cho bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Switch lesson activation
        group.MapPatch("/{id}/activation", SwitchLessonActivationAsync)
            .WithName("SwitchLessonActivation")
            .WithDescription("Kích hoạt hoặc hủy kích hoạt bài học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Reorder lesson
        group.MapPost("/reorder", ReorderLessonAsync)
            .WithName("ReorderLesson")
            .WithDescription("Sắp xếp lại bài học trong section hoặc chuyển sang section khác")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> SwitchLessonActivationAsync(
        Guid id,
        [FromBody] SwitchLessonActivationRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<SwitchLessonActivationRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var apiResult = await lessonService.SwitchLessonActivationAsync(id, request.IsPublished, currentUserService.UserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
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

    private static async Task<IResult> DeleteLessonAsync(
        Guid id,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await lessonService.DeleteLessonAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateVideoLessonAsync(
        [FromBody] CreateVideoLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateVideoLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.CreateVideoLessonAsync(request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> CreateTextLessonAsync(
        [FromBody] CreateTextLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateTextLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.CreateTextLessonAsync(request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> CreateQuizLessonAsync(
        [FromBody] CreateQuizLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateQuizLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.CreateQuizLessonAsync(request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> UpdateVideoLessonAsync(
        Guid id,
        [FromBody] UpdateVideoLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateVideoLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.UpdateVideoLessonAsync(id, request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> UpdateTextLessonAsync(
        Guid id,
        [FromBody] UpdateTextLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateTextLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.UpdateTextLessonAsync(id, request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> UpdateQuizLessonAsync(
        Guid id,
        [FromBody] UpdateQuizLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateQuizLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await lessonService.UpdateQuizLessonAsync(id, request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }


    private static async Task<IResult> ChangeQuizForLessonAsync(
        Guid id,
        [FromBody] ChangeQuizForLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<ChangeQuizForLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var apiResult = await lessonService.ChangeQuizForLessonAsync(id, request.QuizId, currentUserService.UserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> ReorderLessonAsync(
        [FromBody] ReorderLessonRequest request,
        [FromServices] ILessonService lessonService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<ReorderLessonRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var apiResult = await lessonService.ReorderLessonAsync(request, currentUserService.UserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }
}
