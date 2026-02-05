using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Learning.Api.Apis;

public static class EnrollmentApis
{
    public static IEndpointRouteBuilder MapEnrollmentApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/enrollments")
            .MapEnrollmentRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Enrollment Api");

        return builder;
    }

    public static RouteGroupBuilder MapEnrollmentRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/", EnrollFreeAsync)
            .WithName("EnrollFree")
            .WithDescription("Đăng ký khóa học miễn phí")
            .RequireAuthorization()
            .Produces<ApiResponse<EnrollmentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<EnrollmentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/check", CheckEnrollmentAsync)
            .WithName("CheckEnrollment")
            .WithDescription("Kiểm tra user hiện tại đã đăng ký khóa học chưa (dùng bởi Assessment để verify student)")
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/my-course-ids", GetMyEnrolledCourseIdsAsync)
            .WithName("GetMyEnrolledCourseIds")
            .WithDescription("Lấy danh sách ID khóa học đã đăng ký của user hiện tại")
            .RequireAuthorization()
            .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<Guid>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> GetMyEnrolledCourseIdsAsync(
        [FromServices] IEnrollmentService enrollmentService,
        [FromServices] ICurrentUserService currentUserService
    )
    {
        var result = await enrollmentService.GetEnrolledCourseIdsAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CheckEnrollmentAsync(
        [FromQuery] Guid courseId,
        [FromServices] IEnrollmentService enrollmentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await enrollmentService.IsUserEnrolledInCourseAsync(currentUserService.UserId, courseId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> EnrollFreeAsync(
        [FromBody] EnrollFreeRequest request,
        [FromServices] IEnrollmentService enrollmentService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<EnrollFreeRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var userId = currentUserService.UserId;
        var result = await enrollmentService.EnrollFreeAsync(userId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
