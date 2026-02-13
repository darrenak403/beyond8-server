using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Certificates;
using Beyond8.Learning.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace Beyond8.Learning.Api.Apis;

public static class CertificateApis
{
    public static IEndpointRouteBuilder MapCertificateApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/certificates")
            .MapCertificateRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Certificate Api");

        return builder;
    }

    public static RouteGroupBuilder MapCertificateRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetMyCertificatesAsync)
            .WithName("GetMyCertificates")
            .WithDescription("Danh sách chứng chỉ của user hiện tại")
            .RequireAuthorization()
            .Produces<ApiResponse<List<CertificateSimpleResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<CertificateSimpleResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/{id:guid}", GetCertificateByIdAsync)
            .WithName("GetCertificateById")
            .WithDescription("Chi tiết chứng chỉ theo Id (chỉ xem được chứng chỉ của mình)")
            .RequireAuthorization()
            .Produces<ApiResponse<CertificateDetailResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CertificateDetailResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/verify/{hash}", VerifyCertificateAsync)
            .WithName("VerifyCertificate")
            .WithDescription("Kiểm tra mã chứng chỉ (VerificationHash) - xác thực công khai")
            .AllowAnonymous()
            .Produces<ApiResponse<CertificateVerificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CertificateVerificationResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("courses/{courseId:guid}/eligibility-config", GetCertificateEligibilityConfigAsync)
            .WithName("GetCertificateEligibilityConfig")
            .WithDescription("Lấy cấu hình điều kiện cấp chứng chỉ theo khóa học (Instructor - chỉ khóa của mình)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CertificateEligibilityConfigResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CertificateEligibilityConfigResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("courses/{courseId:guid}/eligibility-config", UpdateCertificateEligibilityConfigAsync)
            .WithName("UpdateCertificateEligibilityConfig")
            .WithDescription("Cập nhật cấu hình điều kiện cấp chứng chỉ theo khóa học (Instructor - chỉ khóa của mình)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .Produces<ApiResponse<CertificateEligibilityConfigResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CertificateEligibilityConfigResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetMyCertificatesAsync(
        [FromServices] ICertificateService certificateService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await certificateService.GetMyCertificatesAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCertificateByIdAsync(
        [FromRoute] Guid id,
        [FromServices] ICertificateService certificateService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await certificateService.GetByIdAsync(id, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> VerifyCertificateAsync(
        [FromRoute] string hash,
        [FromServices] ICertificateService certificateService)
    {
        var result = await certificateService.GetByVerificationHashAsync(hash);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetCertificateEligibilityConfigAsync(
        [FromRoute] Guid courseId,
        [FromServices] ICertificateService certificateService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await certificateService.GetCertificateEligibilityConfigAsync(courseId, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateCertificateEligibilityConfigAsync(
        [FromRoute] Guid courseId,
        [FromBody] UpdateCertificateEligibilityConfigRequest request,
        [FromServices] ICertificateService certificateService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<UpdateCertificateEligibilityConfigRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;
        var result = await certificateService.UpdateCertificateEligibilityConfigAsync(courseId, request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
