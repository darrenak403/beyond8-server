using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Learning.Application.Dtos.Certificates;
using Beyond8.Learning.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

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
}
