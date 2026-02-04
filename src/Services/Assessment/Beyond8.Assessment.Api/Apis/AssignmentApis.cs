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
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}", GetAssignmentByIdAsync)
                .WithName("GetAssignmentById")
                .WithDescription("Lấy assignment theo ID")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<AssignmentResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AssignmentResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{id:guid}/student", GetAssignmentByIdForStudentAsync)
                .WithName("GetAssignmentByIdForStudent")
                .WithDescription("Lấy assignment theo ID cho học sinh")
                .RequireAuthorization(x => x.RequireRole(Role.Student))
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/", GetAllAssignmentsAsync)
                .WithName("GetAllAssignments")
                .WithDescription("Lấy tất cả assignments")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<List<AssignmentSimpleResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<AssignmentSimpleResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{id:guid}", DeleteAssignmentAsync)
                .WithName("DeleteAssignment")
                .WithDescription("Xóa assignment")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPut("/{id:guid}", UpdateAssignmentAsync)
                .WithName("UpdateAssignment")
                .WithDescription("Cập nhật assignment")
                .RequireAuthorization(x => x.RequireRole(Role.Instructor))
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AssignmentSimpleResponse>>(StatusCodes.Status400BadRequest)
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

        private static async Task<IResult> GetAssignmentByIdForStudentAsync(
            [FromRoute] Guid id,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentService.GetAssignmentByIdForStudentAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
        private static async Task<IResult> GetAssignmentByIdAsync(
            [FromRoute] Guid id,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentService.GetAssignmentByIdAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetAllAssignmentsAsync(
            [AsParameters] GetAssignmentsRequest request,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<GetAssignmentsRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await assignmentService.GetAllAssignmentsAsync(currentUserService.UserId, request);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteAssignmentAsync(
            [FromRoute] Guid id,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await assignmentService.DeleteAssignmentAsync(id, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> UpdateAssignmentAsync(
            [FromRoute] Guid id,
            [FromBody] UpdateAssignmentRequest request,
            [FromServices] IAssignmentService assignmentService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<UpdateAssignmentRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await assignmentService.UpdateAssignmentAsync(id, request, currentUserService.UserId);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}