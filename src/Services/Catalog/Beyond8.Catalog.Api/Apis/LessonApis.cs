using System;
using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Api.Apis;

public static class LessonApis
{
    public static IEndpointRouteBuilder MapLessonApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/lessons")
            .MapLessonRoutes()
            .WithTags("Lesson Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapLessonRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/video/callback", CallbackHlsAsync)
            .WithName("CallbackHls")
            .WithDescription("Callback để xử lý video HLS")
            .AllowAnonymous()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest);

        // group.MapGet("/", GetAllLessons)
        //     .WithName("GetAllLessons")
        //     .WithDescription("Lấy tất cả bài giảng")
        //     .RequireAuthorization()
        //     .Produces<ApiResponse<List<LessonResponse>>>(StatusCodes.Status200OK)
        //     .Produces<ApiResponse<List<LessonResponse>>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> CallbackHlsAsync(VideoCallbackDto request, ILessonService lessonService)
    {
        var response = await lessonService.CallbackHlsAsync(request);
        return Results.Ok(response);
    }
}
