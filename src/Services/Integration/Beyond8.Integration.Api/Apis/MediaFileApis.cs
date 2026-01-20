using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis;

public static class MediaFileApis
{
    private const string AvatarFolder = "avatars";
    private const string InstructorProfileCertificatesFolder = "instructor/profile/certificates";
    private const string InstructorProfileIdentityCardsFolder = "instructor/profile/identity-cards";

    public static IEndpointRouteBuilder MapMediaFileApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/media")
            .MapMediaFileRoutes()
            .WithTags("Media File Api")
            .RequireAuthorization();

        return builder;
    }

    public static RouteGroupBuilder MapMediaFileRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/avatar/presigned-url", GetAvatarPresignUrlAsync)
            .WithName("GetAvatarPresignedUrl")
            .WithDescription("Lấy presigned URL để upload avatar")
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/certificate/presigned-url", GetCertificatePresignUrlAsync)
            .WithName("GetCertificatePresignedUrl")
            .WithDescription("Lấy presigned URL để upload chứng chỉ")
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/identity-card/front/presigned-url", GetIdentityCardFrontPresignUrlAsync)
            .WithName("GetIdentityCardFrontPresignedUrl")
            .WithDescription("Lấy presigned URL để upload mặt trước CMND/CCCD")
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<UploadFileResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/identity-card/back/presigned-url", GetIdentityCardBackPresignUrlAsync)
            .WithName("GetIdentityCardBackPresignedUrl")
            .WithDescription("Lấy presigned URL để upload mặt sau CMND/CCCD")
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
            .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<MediaFileDto>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapDelete("/{fileId:guid}", DeleteFileAsync)
            .WithName("DeleteFile")
            .WithDescription("Xóa file theo ID")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
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
}
