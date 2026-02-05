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
        group.MapGet("/verify/{hash}", VerifyCertificateAsync)
            .WithName("VerifyCertificate")
            .WithDescription("Kiểm tra mã chứng chỉ (VerificationHash) - xác thực công khai")
            .AllowAnonymous()
            .Produces<ApiResponse<CertificateVerificationResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CertificateVerificationResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> VerifyCertificateAsync(
        [FromRoute] string hash,
        [FromServices] ICertificateService certificateService)
    {
        var result = await certificateService.GetByVerificationHashAsync(hash);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
