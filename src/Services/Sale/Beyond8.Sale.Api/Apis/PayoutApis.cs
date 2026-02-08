using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payouts;
using Beyond8.Sale.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class PayoutApis
{
    public static IEndpointRouteBuilder MapPayoutApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/payouts")
            .MapPayoutRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Payout Api");

        return builder;
    }

    public static RouteGroupBuilder MapPayoutRoutes(this RouteGroupBuilder group)
    {
        // ── Instructor Endpoints ──
        group.MapPost("/request", CreatePayoutRequestAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("CreatePayoutRequest")
            .WithDescription("Yêu cầu rút tiền (tối thiểu 500k VND)")
            .Produces<ApiResponse<object>>(200)
            .Produces(400);

        group.MapGet("/my-requests", GetMyPayoutRequestsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("GetMyPayoutRequests")
            .WithDescription("Lấy danh sách yêu cầu rút tiền của giảng viên (phân trang)")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);

        group.MapGet("/{payoutId:guid}", GetPayoutRequestByIdAsync)
            .RequireAuthorization()
            .WithName("GetPayoutRequestById")
            .WithDescription("Lấy chi tiết yêu cầu rút tiền")
            .Produces<ApiResponse<object>>(200)
            .Produces(404);

        // ── Admin Endpoints ──
        group.MapGet("/", GetAllPayoutRequestsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("GetAllPayoutRequests")
            .WithDescription("Lấy tất cả yêu cầu rút tiền (Admin/Staff, phân trang)")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);

        group.MapPost("/{payoutId:guid}/approve", ApprovePayoutRequestAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("ApprovePayoutRequest")
            .WithDescription("Phê duyệt yêu cầu rút tiền (Admin/Staff)")
            .Produces<ApiResponse<object>>(200)
            .Produces(400);

        group.MapPost("/{payoutId:guid}/reject", RejectPayoutRequestAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("RejectPayoutRequest")
            .WithDescription("Từ chối yêu cầu rút tiền với lý do (Admin/Staff)")
            .Produces<ApiResponse<object>>(200)
            .Produces(400);

        return group;
    }

    // ── Handlers ──

    private static async Task<IResult> CreatePayoutRequestAsync(
        [FromServices] IPayoutService payoutService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<CreatePayoutRequest> validator,
        [FromBody] CreatePayoutRequest request)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        // Ensure instructor creates payout for themselves
        request.InstructorId = currentUserService.UserId;

        var result = await payoutService.CreatePayoutRequestAsync(request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyPayoutRequestsAsync(
        [FromServices] IPayoutService payoutService,
        [FromServices] ICurrentUserService currentUserService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await payoutService.GetPayoutRequestsByInstructorAsync(
            currentUserService.UserId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPayoutRequestByIdAsync(
        Guid payoutId,
        [FromServices] IPayoutService payoutService)
    {
        var result = await payoutService.GetPayoutRequestByIdAsync(payoutId);
        return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetAllPayoutRequestsAsync(
        [FromServices] IPayoutService payoutService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await payoutService.GetPayoutRequestsAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ApprovePayoutRequestAsync(
        Guid payoutId,
        [FromServices] IPayoutService payoutService)
    {
        var result = await payoutService.ApprovePayoutRequestAsync(payoutId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RejectPayoutRequestAsync(
        Guid payoutId,
        [FromServices] IPayoutService payoutService,
        [FromBody] RejectPayoutDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
            return Results.BadRequest(ApiResponse<bool>.FailureResponse("Lý do từ chối không được để trống"));

        var result = await payoutService.RejectPayoutRequestAsync(payoutId, request.Reason);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}

/// <summary>
/// DTO for reject payout request body
/// </summary>
public class RejectPayoutDto
{
    public string Reason { get; set; } = string.Empty;
}
