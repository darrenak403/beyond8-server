using Beyond8.Catalog.Application.Clients.Identity;
using Beyond8.Catalog.Application.Dtos.Courses;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Catalog.Api.Apis;

public static class CourseApis
{
    public static IEndpointRouteBuilder MapCourseApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/courses")
            .MapCourseRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Course Api");

        return builder;
    }

    public static RouteGroupBuilder MapCourseRoutes(this RouteGroupBuilder group)
    {
        // Public Search Operations
        group.MapGet("/", GetAllCoursesAsync)
            .WithName("GetAllCourses")
            .WithDescription("Lấy danh sách tất cả khóa học")
            .Produces<ApiResponse<List<CourseSimpleResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status400BadRequest);

        // Full-Text Search endpoint
        group.MapGet("/search", FullTextSearchCoursesAsync)
            .WithName("FullTextSearchCourses")
            .WithDescription("Tìm kiếm khóa học sử dụng Full-Text Search. Hỗ trợ tiếng Việt không dấu (VD: 'lap trinh' tìm được 'Lập trình'). Kết quả được xếp hạng theo độ liên quan.")
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}/summary", GetCourseSummaryAsync)
            .WithName("GetCourseSummary")
            .WithDescription("Lấy tóm tắt khóa học")
            .Produces<ApiResponse<CourseSummaryResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseSummaryResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}/details", GetCourseDetailsAsync)
            .WithName("GetCourseByIdDetails")
            .WithDescription("Lấy thông tin khóa học chi tiết theo ID cho học viên")
            .RequireAuthorization()
            .Produces<ApiResponse<CourseDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseDetailResponse>>(StatusCodes.Status400BadRequest);

        // Instructor Operations
        group.MapPost("/", CreateCourseAsync)
            .WithName("CreateCourse")
            .WithDescription("Tạo khóa học mới")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id}", GetCourseByIdAsync)
            .WithName("GetCourseById")
            .WithDescription("Lấy thông tin khóa học theo ID")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{id}", DeleteCourseAsync)
            .WithName("DeleteCourse")
            .WithDescription("Xóa khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/instructor", GetCoursesByInstructorAsync)
            .WithName("GetCoursesByInstructor")
            .WithDescription("Lấy danh sách khóa học của giảng viên")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/instructor/stats", GetCourseStatsByInstructorAsync)
            .WithName("GetCourseStatsByInstructor")
            .WithDescription("Lấy thống kê khóa học của giảng viên")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseStatsResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseStatsResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/submit-approval", SubmitForApprovalAsync)
            .WithName("SubmitForApproval")
            .WithDescription("Nộp duyệt khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPatch("/{id}/metadata", UpdateCourseMetadataAsync)
            .WithName("UpdateCourseMetadata")
            .WithDescription("Cập nhật thông tin cơ bản khóa học (không cần duyệt lại)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CourseResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/admin", GetAllCoursesForAdminAsync)
            .WithName("GetAllCoursesForAdminAsync")
            .WithDescription("Lấy danh sách khóa học chờ phê duyệt")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CourseResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/approve", ApproveCourseAsync)
            .WithName("ApproveCourse")
            .WithDescription("Phê duyệt khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/reject", RejectCourseAsync)
            .WithName("RejectCourse")
            .WithDescription("Từ chối khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/publish", PublishCourseAsync)
            .WithName("PublishCourse")
            .WithDescription("Công bố khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/{id}/unpublish", UnpublishCourseAsync)
            .WithName("UnpublishCourse")
            .WithDescription("Ẩn khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/{id}/thumbnail", UpdateCourseThumbnailAsync)
            .WithName("UpdateCourseThumbnail")
            .WithDescription("Cập nhật ảnh đại diện khóa học")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> UpdateCourseThumbnailAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateCourseThumbnailRequest request,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<bool>.FailureResponse(message));
        }

        var result = await courseService.UpdateCourseThumbnailAsync(id, currentUserService.UserId, request);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetAllCoursesAsync(
        [FromServices] ICourseService courseService,
        [AsParameters] PaginationCourseSearchRequest pagination)
    {
        var result = await courseService.GetAllCoursesAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> FullTextSearchCoursesAsync(
        [FromServices] ICourseService courseService,
        [AsParameters] FullTextSearchRequest request)
    {
        var result = await courseService.FullTextSearchCoursesAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseSummaryAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService)
    {
        var result = await courseService.GetCourseSummaryAsync(id);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseDetailsAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await courseService.GetCourseDetailsAsync(id, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateCourseAsync(
        [FromBody] CreateCourseRequest request,
        [FromServices] ICourseService courseService,
        [FromServices] IValidator<CreateCourseRequest> validator,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<CourseResponse>.FailureResponse(message));
        }

        var result = await courseService.CreateCourseAsync(request, currentUserId);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var currentUserId = currentUserService.UserId;
        var result = await courseService.GetCourseByIdAsync(id, currentUserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> DeleteCourseAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<bool>.FailureResponse(message));
        }

        var result = await courseService.DeleteCourseAsync(id, currentUserId);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCoursesByInstructorAsync(
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [AsParameters] PaginationCourseInstructorSearchRequest pagination)
    {
        var instructorId = currentUserService.UserId;
        var result = await courseService.GetCoursesByInstructorAsync(instructorId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCourseStatsByInstructorAsync(
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var instructorId = currentUserService.UserId;
        var result = await courseService.GetCourseStatsByInstructorAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> SubmitForApprovalAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<bool>.FailureResponse(message));
        }

        var result = await courseService.SubmitForApprovalAsync(id, currentUserId);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> GetAllCoursesForAdminAsync(
        [FromServices] ICourseService courseService,
        [AsParameters] PaginationCourseAdminSearchRequest pagination)
    {
        var result = await courseService.GetAllCoursesForAdminAsync(pagination);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> ApproveCourseAsync(
        [FromRoute] Guid id,
        [FromBody] ApproveCourseRequest request,
        [FromServices] ICourseService courseService,
        [FromServices] IValidator<ApproveCourseRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await courseService.ApproveCourseAsync(id, request);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> RejectCourseAsync(
        [FromRoute] Guid id,
        [FromBody] RejectCourseRequest request,
        [FromServices] ICourseService courseService,
        [FromServices] IValidator<RejectCourseRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await courseService.RejectCourseAsync(id, request);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> PublishCourseAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<bool>.FailureResponse(message));
        }

        var result = await courseService.PublishCourseAsync(id, currentUserId);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<IResult> UnpublishCourseAsync(
        [FromRoute] Guid id,
        [FromServices] ICourseService courseService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<bool>.FailureResponse(message));
        }

        var result = await courseService.UnpublishCourseAsync(id, currentUserId);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }
    private static async Task<IResult> UpdateCourseMetadataAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateCourseMetadataRequest request,
        [FromServices] ICourseService courseService,
        [FromServices] IValidator<UpdateCourseMetadataRequest> validator,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var currentUserId = currentUserService.UserId;

        // Check instructor verification before proceeding
        var (isVerified, message) = await CheckInstructorVerificationAsync(currentUserService, identityClient);
        if (!isVerified)
        {
            return Results.BadRequest(ApiResponse<CourseResponse>.FailureResponse(message));
        }

        var result = await courseService.UpdateCourseMetadataAsync(id, currentUserId, request);
        return result.IsSuccess
            ? Results.Ok(result)
            : Results.BadRequest(result);
    }

    private static async Task<(bool IsVerified, string Message)> CheckInstructorVerificationAsync(
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IIdentityClient identityClient)
    {
        var currentUserId = currentUserService.UserId;
        var verificationResponse = await identityClient.CheckInstructorProfileVerifiedAsync(currentUserId);
        if (!verificationResponse.IsSuccess || !verificationResponse.Data)
        {
            return (false, "Hồ sơ giảng viên chưa được phê duyệt.");
        }
        return (true, string.Empty);
    }
}