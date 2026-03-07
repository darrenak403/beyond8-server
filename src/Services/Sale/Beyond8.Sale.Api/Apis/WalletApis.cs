using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Application.Helpers;
using Beyond8.Sale.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Beyond8.Sale.Api.Apis;

public static class WalletApis
{
    public static IEndpointRouteBuilder MapWalletApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/wallets")
            .MapWalletRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Wallet Api");

        return builder;
    }

    public static RouteGroupBuilder MapWalletRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/my-wallet", GetMyWalletAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("GetMyWallet")
            .WithDescription("Lấy thông tin ví của giảng viên hiện tại (Instructor)")
            .Produces<ApiResponse<InstructorWalletResponse>>(200)
            .Produces(401);

        group.MapPost("/top-up", TopUpWalletAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("TopUpWallet")
            .WithDescription("Nạp tiền vào ví giảng viên qua VNPay — Purpose: WalletTopUp (Instructor)")
            .Produces<ApiResponse<PaymentUrlResponse>>(200)
            .Produces(400);

        group.MapGet("/my-wallet/transactions", GetMyWalletTransactionsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("GetMyWalletTransactions")
            .WithDescription("Lấy lịch sử giao dịch ví của giảng viên hiện tại (Instructor, phân trang)")
            .Produces<ApiResponse<List<WalletTransactionResponse>>>(200)
            .Produces(401);

        // ── Admin Endpoints ──
        group.MapGet("/instructor/{instructorId:guid}", GetWalletByInstructorAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("GetWalletByInstructor")
            .WithDescription("Lấy thông tin ví của một giảng viên cụ thể (Admin/Staff)")
            .Produces<ApiResponse<InstructorWalletResponse>>(200)
            .Produces(401);

        group.MapGet("/instructor/{instructorId:guid}/transactions", GetWalletTransactionsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("GetWalletTransactions")
            .WithDescription("Lấy lịch sử giao dịch ví của một giảng viên cụ thể (Admin/Staff, phân trang)")
            .Produces<ApiResponse<List<WalletTransactionResponse>>>(200)
            .Produces(401);

        // ── Internal: Create wallet (Admin/Staff) ──
        group.MapPost("/create/{instructorId:guid}", CreateWalletAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("CreateWallet")
            .WithDescription("Tạo ví cho một giảng viên (Admin/Staff - Internal use)")
            .Produces<ApiResponse<InstructorWalletResponse>>(200)
            .Produces(401);

        return group;
    }

    // ── Handlers ──

    private static async Task<IResult> GetMyWalletAsync(
        [FromServices] IInstructorWalletService walletService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await walletService.GetWalletByInstructorAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyWalletTransactionsAsync(
        [FromServices] IInstructorWalletService walletService,
        [FromServices] ICurrentUserService currentUserService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await walletService.GetWalletTransactionsAsync(currentUserService.UserId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetWalletByInstructorAsync(
        Guid instructorId,
        [FromServices] IInstructorWalletService walletService)
    {
        var result = await walletService.GetWalletByInstructorAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetWalletTransactionsAsync(
        Guid instructorId,
        [FromServices] IInstructorWalletService walletService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await walletService.GetWalletTransactionsAsync(instructorId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CreateWalletAsync(
        Guid instructorId,
        [FromServices] IInstructorWalletService walletService)
    {
        var result = await walletService.CreateWalletAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> TopUpWalletAsync(
        [FromServices] IPaymentService paymentService,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IValidator<TopUpRequest> validator,
        [FromServices] IOptions<VNPaySettings> vnPayOptions,
        [FromBody] TopUpRequest request,
        HttpContext httpContext)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var callbackUrl = $"{vnPayOptions.Value.BackendCallbackUrl.TrimEnd('/')}/api/v1/payments/vnpay/callback";

        var result = await paymentService.ProcessTopUpAsync(
            currentUserService.UserId, request.Amount, callbackUrl, ipAddress);

        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
