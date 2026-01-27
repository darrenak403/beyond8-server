using Beyond8.Common.Extensions;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.VnptEkyc;
using Beyond8.Integration.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis;

public static class VnptEkycApis
{
    public static IEndpointRouteBuilder MapVnptEkycApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/vnpt-ekyc")
            .MapVnptEkycRoutes()
            .WithTags("VNPT eKYC Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    private static RouteGroupBuilder MapVnptEkycRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/is-identity-card", UploadAndCheckLivenessAsync)
            .WithName("IsIdentityCard")
            .WithDescription("Kiểm tra xem ảnh có phải là ảnh CMND/CCCD/Hộ chiếu không (cần xác thực làm việc trên máy chính)")
            .RequireAuthorization()
            .DisableAntiforgery()
            .Produces<ApiResponse<LivenessResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<LivenessResponse>>(StatusCodes.Status400BadRequest);

        group.MapPost("/classify", ClassifyAsync)
            .WithName("ClassifyIdCard")
            .WithDescription("Phân loại ảnh CMND/CCCD/Hộ chiếu và trích xuất thông tin. Mặt trước: loại giấy tờ và số giấy tờ. Mặt sau: ngày hết hạn (cần xác thực làm việc trên máy chính)")
            .RequireAuthorization()
            .Produces<ApiResponse<ClassifyWithOcrResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<ClassifyWithOcrResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> UploadAndCheckLivenessAsync(
        IFormFile file,
        [FromServices] IVnptEkycService vnptEkycService)
    {
        var result = await vnptEkycService.UploadAndCheckLivenessAsync(file);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ClassifyAsync(
        [FromBody] ClassifyRequest request,
        [FromServices] IVnptEkycService vnptEkycService,
        [FromServices] IValidator<ClassifyRequest> validator)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await vnptEkycService.ClassifyAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
