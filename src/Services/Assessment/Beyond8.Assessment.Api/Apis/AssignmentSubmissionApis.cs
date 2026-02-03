using Beyond8.Assessment.Application.Dtos.AssignmentSubmissions;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Assessment.Application.Validators.AssignmentSubmissions;
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

            return group;
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
    }
}