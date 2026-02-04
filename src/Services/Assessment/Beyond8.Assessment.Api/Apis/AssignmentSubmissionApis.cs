using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis
{
    public static class AssignmentSubmissionApis
    {
        public static IEndpointRouteBuilder MapAssignmentSubmissionApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/assignment-submissions")
                .MapAssignmentSubmissionRoutes()
                .WithTags("Assignment Submission Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapAssignmentSubmissionRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/{assignmentId:guid}", CreateSubmissionAsync)
                .WithName("CreateSubmission")
                .WithDescription("Tạo submission cho assignment")
                .RequireAuthorization(x => x.RequireRole(Role.Student))
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPatch("/{submissionId:guid}/instructor-grade", InstructorGradingSubmissionAsync)
                .WithName("InstructorGradingSubmission")
                .WithDescription("Chấm điểm submission bởi giảng viên")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("{submissionId:guid}/student", GetSubmissionByIdAsync)
                .WithName("GetSubmissionByIdForStudent")
                .WithDescription("Lấy submission theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Student))
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/assignment/{assignmentId:guid}/student", GetSubmissionsByAssignmentIdAsync)
                .WithName("GetSubmissionsByAssignmentId")
                .WithDescription("Lấy danh sách submission theo assignment ID")
                .RequireAuthorization(x => x.RequireRole(Role.Student))
                .Produces<ApiResponse<List<SubmissionResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<SubmissionResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/instructor", GetAllSubmissionsByInstructorAsync)
                .WithName("GetAllSubmissions")
                .WithDescription("Lấy danh sách tất cả submission")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<SubmissionResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<SubmissionResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("{submissionId:guid}/instructor", GetSubmissionByIdForInstructorAsync)
                .WithName("GetSubmissionByIdForInstructor")
                .WithDescription("Lấy submission theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<SubmissionResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetSubmissionByIdForInstructorAsync(
            [FromRoute] Guid submissionId,
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentSubmissionService.GetSubmissionByIdForInstructorAsync(submissionId, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }


        private static async Task<IResult> GetAllSubmissionsByInstructorAsync(
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentSubmissionService.GetAllSubmissionsByInstructorAsync(currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetSubmissionsByAssignmentIdAsync(
            [FromRoute] Guid assignmentId,
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentSubmissionService.GetSubmissionsByAssignmentIdAsync(assignmentId, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetSubmissionByIdAsync(
            [FromRoute] Guid submissionId,
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentSubmissionService.GetSubmissionByIdAsync(submissionId, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> CreateSubmissionAsync(
            [FromRoute] Guid assignmentId,
            [FromBody] CreateSubmissionRequest request,
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<CreateSubmissionRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await assignmentSubmissionService.CreateSubmissionAsync(assignmentId, request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> InstructorGradingSubmissionAsync(
            [FromRoute] Guid submissionId,
            [FromBody] GradeSubmissionRequest request,
            [FromServices] IAssignmentSubmissionService assignmentSubmissionService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<GradeSubmissionRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await assignmentSubmissionService.InstructorGradingSubmissionAsync(submissionId, request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}