using Beyond8.Assessment.Application.Dtos.Reassign;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis;

public static class ReassignApis
{
    public static IEndpointRouteBuilder MapReassignApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/reassign")
            .MapReassignRoutes()
            .WithTags("Reassign Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapReassignRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/quiz/{quizId:guid}/request", RequestQuizReassignAsync)
            .WithName("RequestQuizReassign")
            .WithDescription("Học sinh yêu cầu reassign (reset lượt làm) quiz")
            .RequireAuthorization(x => x.RequireRole(Role.Student))
            .Produces<ApiResponse<ReassignRequestResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ReassignRequestResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/assignment/{assignmentId:guid}/request", RequestAssignmentReassignAsync)
            .WithName("RequestAssignmentReassign")
            .WithDescription("Học sinh yêu cầu reassign (reset lượt nộp) assignment")
            .RequireAuthorization(x => x.RequireRole(Role.Student))
            .Produces<ApiResponse<ReassignRequestResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ReassignRequestResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/instructor/overview", GetOverviewForInstructorAsync)
            .WithName("GetReassignOverviewForInstructor")
            .WithDescription("Instructor xem tổng quan yêu cầu reassign (pending), có phân trang và tìm kiếm. Duyệt qua 2 endpoint reset bên dưới")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<ReassignRequestItemDto>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/quiz/{quizId:guid}/reset/{studentId:guid}", ResetQuizAttemptsAsync)
            .WithName("ResetQuizAttempts")
            .WithDescription("Instructor reset lượt làm quiz cho student để làm lại")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/assignment/{assignmentId:guid}/reset/{studentId:guid}", ResetAssignmentSubmissionsAsync)
            .WithName("ResetAssignmentSubmissions")
            .WithDescription("Instructor reset lượt nộp assignment cho student để nộp lại")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> RequestQuizReassignAsync(
        [FromRoute] Guid quizId,
        [FromBody] RequestQuizReassignRequest request,
        [FromServices] IReassignService reassignService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<RequestQuizReassignRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await reassignService.RequestQuizReassignAsync(quizId, request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RequestAssignmentReassignAsync(
        [FromRoute] Guid assignmentId,
        [FromBody] RequestAssignmentReassignRequest request,
        [FromServices] IReassignService reassignService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<RequestAssignmentReassignRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await reassignService.RequestAssignmentReassignAsync(assignmentId, request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetOverviewForInstructorAsync(
        [AsParameters] GetReassignOverviewRequest request,
        [FromServices] IReassignService reassignService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<GetReassignOverviewRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await reassignService.GetOverviewForInstructorAsync(currentUserService.UserId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ResetQuizAttemptsAsync(
        [FromRoute] Guid quizId,
        [FromRoute] Guid studentId,
        [FromServices] IQuizAttemptService quizAttemptService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await quizAttemptService.ResetQuizAttemptsForStudentAsync(quizId, studentId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ResetAssignmentSubmissionsAsync(
        [FromRoute] Guid assignmentId,
        [FromRoute] Guid studentId,
        [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await assignmentSubmissionService.ResetSubmissionsForStudentAsync(assignmentId, studentId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
