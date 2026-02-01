using Beyond8.Assessment.Application.Dtos.Assignments;
using Beyond8.Assessment.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Assessment.Api.Apis
{
    public static class AssignmentApis
    {
        public static IEndpointRouteBuilder MapAssignmentApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/assignments")
                .MapAssignmentRoutes()
                .WithTags("Assignment Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapAssignmentRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/", CreateAssignmentAsync)
                .WithName("CreateAssignment")
                .WithDescription("Tạo assignment")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<AssignmentResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AssignmentResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> CreateAssignmentAsync(
            [FromBody] CreateAssignmentRequest request,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<CreateAssignmentRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await assignmentService.CreateAssignmentAsync(request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}