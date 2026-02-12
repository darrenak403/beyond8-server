using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Enrollments;
using Beyond8.Learning.Application.Dtos.Progress;
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
        group.MapPut("/lesson/{lessonId:guid}/heartbeat", UpdateLessonProgressAsync)
            .WithName("UpdateLessonProgress")
            .WithDescription("Cập nhật tiến độ bài học (heartbeat): vị trí xem / đánh dấu hoàn thành")
            .RequireAuthorization()
            .Produces<ApiResponse<LessonProgressResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonProgressResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}/lesson/{lessonId:guid}", GetLessonProgressAsync)
            .WithName("GetLessonProgress")
            .WithDescription("Lấy tiến độ bài học")
            .RequireAuthorization()
            .Produces<ApiResponse<LessonProgressResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LessonProgressResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

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

        group.MapGet("/check-certificate", CheckCertificateAsync)
            .WithName("CheckCertificate")
            .WithDescription("Kiểm tra student đã được cấp certificate cho khóa học chưa (dùng bởi Assessment)")
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

        group.MapGet("/me", GetMyEnrolledCoursesAsync)
            .WithName("GetMyEnrolledCourses")
            .WithDescription("Lấy danh sách khóa học đã đăng ký của user hiện tại")
            .RequireAuthorization()
            .Produces<ApiResponse<List<EnrollmentSimpleResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<EnrollmentSimpleResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}/curriculum-progress", GetCurriculumProgressByEnrollmentIdAsync)
            .WithName("GetCurriculumProgressByEnrollmentId")
            .WithDescription("Lấy tiến độ khung chương trình theo enrollment Id")
            .RequireAuthorization()
            .Produces<ApiResponse<CurriculumProgressResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CurriculumProgressResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetEnrollmentByIdAsync)
            .WithName("GetEnrollmentById")
            .WithDescription("Lấy thông tin khóa học đã đăng ký theo ID")
            .RequireAuthorization()
            .Produces<ApiResponse<EnrollmentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<EnrollmentResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> UpdateLessonProgressAsync(
        [FromRoute] Guid lessonId,
        [FromBody] LessonProgressHeartbeatRequest request,
        [FromServices] IProgressService progressService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<LessonProgressHeartbeatRequest> validator)
    {
        if (request != null && !request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await progressService.UpdateLessonProgressAsync(
            lessonId,
            currentUserService.UserId,
            request ?? new LessonProgressHeartbeatRequest());
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetLessonProgressAsync(
        [FromRoute] Guid id,
        [FromRoute] Guid lessonId,
        [FromServices] IProgressService progressService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await progressService.GetLessonProgressAsync(id, lessonId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCurriculumProgressByEnrollmentIdAsync(
        [FromRoute] Guid id,
        [FromServices] IProgressService progressService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await progressService.GetCurriculumProgressByEnrollmentIdAsync(id, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetEnrollmentByIdAsync(
        [FromRoute] Guid id,
        [FromServices] IEnrollmentService enrollmentService,
        [FromServices] ICurrentUserService currentUserService
    )
    {
        var result = await enrollmentService.GetEnrollmentByIdAsync(id, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyEnrolledCoursesAsync(
        [FromServices] IEnrollmentService enrollmentService,
        [FromServices] ICurrentUserService currentUserService
    )
    {
        var result = await enrollmentService.GetEnrolledCoursesAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
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

    private static async Task<IResult> CheckCertificateAsync(
        [FromQuery] Guid courseId,
        [FromQuery] Guid studentId,
        [FromServices] IEnrollmentService enrollmentService)
    {
        var result = await enrollmentService.HasCertificateForCourseAsync(studentId, courseId);
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
