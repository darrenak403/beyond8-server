using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Common.Security;
using Beyond8.Integration.Application.Dtos.MediaFiles;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Application.Validators.MediaFiles;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis
{
    public static class MediaFileApis
    {
        private const string AvatarFolder = "user/avatars";
        private const string CoverFolder = "user/covers";
        private const string InstructorProfileCertificatesFolder = "instructor/profile/certificates";
        private const string InstructorProfileIdentityCardsFolder = "instructor/profile/identity-cards";
        private const string InstructorProfileVideosFolder = "instructor/profile/intro-videos";
        private const string CourseThumbnailFolder = "course/thumbnails";
        private const string CourseDocumentFolder = "course/documents";
        private const string CourseAssignmentSubmissionFolder = "course/sections/assignments/submissions";
        private const string CourseAssignmentRubricFolder = "course/sections/assignments/rubrics";
        private const string CourseVideoFolder = "course/sections/lessons/videos";


        public static IEndpointRouteBuilder MapMediaFileApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/media")
                .MapMediaFileRoutes()
                .WithTags("Media File Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        public static RouteGroupBuilder MapMediaFileRoutes(this RouteGroupBuilder group)
        {
            group.MapPost("/avatar/presigned-url", GetAvatarPresignUrlAsync)
                .WithName("GetAvatarPresignedUrl")
                .WithDescription("Lấy presigned URL để upload avatar")
                .RequireAuthorization()
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/cover/presigned-url", GetCoverPresignUrlAsync)
                .WithName("GetCoverPresignedUrl")
                .WithDescription("Lấy presigned URL để upload cover")
                .RequireAuthorization()
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/intro-video/presigned-url", GetIntroVideoPresignUrlAsync)
                .WithName("GetIntroVideoPresignedUrl")
                .WithDescription("Lấy presigned URL để upload video giới thiệu")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/certificate/presigned-url", GetCertificatePresignUrlAsync)
                .WithName("GetCertificatePresignedUrl")
                .WithDescription("Lấy presigned URL để upload chứng chỉ")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/identity-card/front/presigned-url", GetIdentityCardFrontPresignUrlAsync)
                .WithName("GetIdentityCardFrontPresignedUrl")
                .WithDescription("Lấy presigned URL để upload mặt trước CMND/CCCD")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/identity-card/back/presigned-url", GetIdentityCardBackPresignUrlAsync)
                .WithName("GetIdentityCardBackPresignedUrl")
                .WithDescription("Lấy presigned URL để upload mặt sau CMND/CCCD")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/course-thumbnail/presigned-url", GetCourseThumbnailPresignUrlAsync)
                .WithName("GetCourseThumbnailPresignedUrl")
                .WithDescription("Lấy presigned URL để upload thumbnail khóa học")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/course-video/presigned-url", GetCourseVideoPresignUrlAsync)
                .WithName("GetCourseVideoPresignedUrl")
                .WithDescription("Lấy presigned URL để upload video khóa học")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/course-document/presigned-url", GetCourseDocumentPresignUrlAsync)
                .WithName("GetCourseDocumentPresignedUrl")
                .WithDescription("Lấy presigned URL để upload document khóa học")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/assignment-submission/presigned-url", GetAssignmentSubmissionPresignUrlAsync)
                .WithName("GetAssignmentSubmissionPresignedUrl")
                .WithDescription("Lấy presigned URL để upload submission assignment")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/assignment-rubric/presigned-url", GetAssignmentRubricPresignUrlAsync)
                .WithName("GetAssignmentRubricPresignedUrl")
                .WithDescription("Lấy presigned URL để upload rubric assignment")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapPost("/confirm", ConfirmUploadAsync)
                .WithName("ConfirmUpload")
                .WithDescription("Xác nhận file đã upload lên S3")
                .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/{fileId:guid}", GetFileByIdAsync)
                .WithName("GetFileById")
                .WithDescription("Lấy thông tin file theo ID")
                .RequireAuthorization()
                .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapDelete("/{fileId:guid}", DeleteFileAsync)
                .WithName("DeleteFile")
                .WithDescription("Xóa file theo ID")
                .RequireAuthorization()
                .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            // New endpoints for Course Catalog service
            group.MapGet("/file-info", GetFileInfoByCloudFrontUrlAsync)
                .WithName("GetFileInfoByCloudFrontUrl")
                .WithDescription("Lấy thông tin file từ CloudFront URL")
                .RequireAuthorization()
                .Produces<ApiResponse<MediaFileInfoDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<MediaFileInfoDto>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/download", GetDownloadUrlAsync)
                .WithName("GetDownloadUrl")
                .WithDescription("Tạo presigned URL để download file")
                .RequireAuthorization()
                .Produces<ApiResponse<DownloadUrlDto>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<DownloadUrlDto>>(StatusCodes.Status404NotFound)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetAssignmentRubricPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForAssignmentRubric();

            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;
            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CourseAssignmentRubricFolder);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetAssignmentSubmissionPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForAssignmentSubmission();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CourseAssignmentSubmissionFolder);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetCourseDocumentPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForCourseDocument();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;
            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CourseDocumentFolder);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetCourseVideoPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForCourseVideo();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CourseVideoFolder);
            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetIntroVideoPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            {
                var validator = UploadFileRequestValidator.ForIntroVideo();
                if (!request.ValidateRequest(validator, out var validationResult))
                    return validationResult!;

                var result = await mediaFileService.InitiateUploadAsync(
                    currentUserService.UserId,
                    request,
                    InstructorProfileVideosFolder);

                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
            }
        }

        private static async Task<IResult> GetCoverPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForCover();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CoverFolder);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetAvatarPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForAvatar();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                AvatarFolder);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetCertificatePresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForDocument();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                InstructorProfileCertificatesFolder);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetIdentityCardFrontPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForDocument();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                InstructorProfileIdentityCardsFolder,
                "front");

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetIdentityCardBackPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForDocument();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                InstructorProfileIdentityCardsFolder,
                "back");

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetCourseThumbnailPresignUrlAsync(
            [FromBody] UploadFileRequest request,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var validator = UploadFileRequestValidator.ForDocument();
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.InitiateUploadAsync(
                currentUserService.UserId,
                request,
                CourseThumbnailFolder);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> ConfirmUploadAsync(
        [FromBody] ConfirmUploadRequest request,
        [FromServices] IMediaFileService mediaFileService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<ConfirmUploadRequest> validator)
        {
            if (!request.ValidateRequest(validator, out var validationResult))
                return validationResult!;

            var result = await mediaFileService.ConfirmUploadAsync(
                currentUserService.UserId,
                request);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetFileByIdAsync(
            [FromRoute] Guid fileId,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await mediaFileService.GetFileByIdAsync(
                currentUserService.UserId,
                fileId);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> DeleteFileAsync(
            [FromRoute] Guid fileId,
            [FromServices] IMediaFileService mediaFileService,
            [FromServices] ICurrentUserService currentUserService)
        {
            var result = await mediaFileService.DeleteFileAsync(
                currentUserService.UserId,
                fileId);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetFileInfoByCloudFrontUrlAsync(
            [FromQuery] string cloudFrontUrl,
            [FromServices] IMediaFileService mediaFileService)
        {
            if (string.IsNullOrWhiteSpace(cloudFrontUrl))
            {
                return Results.BadRequest(ApiResponse<MediaFileInfoDto>.FailureResponse(
                    "CloudFront URL là bắt buộc"));
            }

            var result = await mediaFileService.GetFileInfoByCloudFrontUrlAsync(cloudFrontUrl);

            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }

        private static async Task<IResult> GetDownloadUrlAsync(
            [FromQuery] string cloudFrontUrl,
            [FromQuery] bool inline,
            [FromServices] IMediaFileService mediaFileService)
        {
            if (string.IsNullOrWhiteSpace(cloudFrontUrl))
            {
                return Results.BadRequest(ApiResponse<DownloadUrlDto>.FailureResponse(
                    "CloudFront URL là bắt buộc"));
            }

            var result = await mediaFileService.GetDownloadUrlAsync(cloudFrontUrl, inline);

            return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
        }
    }
}
