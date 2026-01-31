using Beyond8.Catalog.Application.Dtos.Sections;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

public static class SectionApis
{
    public static IEndpointRouteBuilder MapSectionApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/sections")
            .MapSectionRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Section Api");

        return builder;
    }

    public static RouteGroupBuilder MapSectionRoutes(this RouteGroupBuilder group)
    {
        // Get sections by course
        group.MapGet("/course/{courseId}", GetSectionsByCourseIdAsync)
            .WithName("GetSectionsByCourseId")
            .WithDescription("Lấy danh sách chương của khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<SectionResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<SectionResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Get section by id
        group.MapGet("/{id}", GetSectionByIdAsync)
            .WithName("GetSectionById")
            .WithDescription("Lấy thông tin chương theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Create section
        group.MapPost("/", CreateSectionAsync)
            .WithName("CreateSection")
            .WithDescription("Tạo chương mới")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Update section
        group.MapPatch("/{id}", UpdateSectionAsync)
            .WithName("UpdateSection")
            .WithDescription("Cập nhật thông tin chương")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SectionResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // Delete section
        group.MapDelete("/{id}", DeleteSectionAsync)
            .WithName("DeleteSection")
            .WithDescription("Xóa chương")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id}/assignment", ChangeAssignmentForSectionAsync)
            .WithName("ChangeAssignmentForSection")
            .WithDescription("Thay đổi assignment khác cho chương")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor)
            )
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetSectionsByCourseIdAsync(
        Guid courseId,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await sectionService.GetSectionsByCourseIdAsync(courseId, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetSectionByIdAsync(
        Guid id,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await sectionService.GetSectionByIdAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateSectionAsync(
        [FromBody] CreateSectionRequest request,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreateSectionRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await sectionService.CreateSectionAsync(request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> UpdateSectionAsync(
        Guid id,
        [FromBody] UpdateSectionRequest request,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateSectionRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await sectionService.UpdateSectionAsync(id, request, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }

    private static async Task<IResult> DeleteSectionAsync(
        Guid id,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await sectionService.DeleteSectionAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ChangeAssignmentForSectionAsync(
        Guid id,
        [FromBody] ChangeAssignmentForSectionRequest request,
        [FromServices] ISectionService sectionService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<ChangeAssignmentForSectionRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var result))
            return result!;

        var currentUserId = currentUserService.UserId;
        var apiResult = await sectionService.ChangeAssignmentForSectionAsync(id, request.AssignmentId, currentUserId);
        return apiResult.IsSuccess ? Results.Ok(apiResult) : Results.BadRequest(apiResult);
    }
}
