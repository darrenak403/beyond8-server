using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Instructors;
using Beyond8.Identity.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Identity.Api.Apis
{
    public static class InstructorApis
    {
        public static IEndpointRouteBuilder MapInstructorApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/instructors")
                .MapInstructorRoutes()
                .WithTags("Instructor Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapInstructorRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/registration", SubmitInstructorApplicationAsync)
                 .WithName("SubmitInstructorApplication")
                 .WithDescription("Gửi đơn đăng ký trở thành giảng viên")
                 .RequireAuthorization()
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status200OK)
                 .Produces<ApiResponse<InstructorProfileResponse>>(StatusCodes.Status400BadRequest)
                 .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> SubmitInstructorApplicationAsync(
            [FromBody] CreateInstructorProfileRequest request,
            [FromServices] IInstructorService instructorService,
            [FromServices] ICurrentUserService currentUserService,
            [FromServices] IValidator<CreateInstructorProfileRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return Results.BadRequest(validationResult);

            var response = await instructorService.SubmitInstructorApplicationAsync(request, currentUserService.UserId);

            return response.IsSuccess
                            ? Results.Created($"/api/v1/instructors/applications/{response.Data!.Id}", response)
                            : Results.BadRequest(response);

        }
    }
}
