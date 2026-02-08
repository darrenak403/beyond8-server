using Beyond8.Common;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class TransactionApis
{
    public static IEndpointRouteBuilder MapTransactionApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/transactions")
            .MapTransactionRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Transaction Api");

        return builder;
    }

    public static RouteGroupBuilder MapTransactionRoutes(this RouteGroupBuilder group)
    {
        // ── Admin Endpoints ──
        group.MapGet("/", GetAllTransactionsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("GetAllTransactions")
            .WithDescription("Lấy tất cả giao dịch (Admin/Staff, phân trang)")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);

        group.MapGet("/{transactionId:guid}", GetTransactionByIdAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff, Role.Instructor))
            .WithName("GetTransactionById")
            .WithDescription("Lấy chi tiết giao dịch (Admin/Staff/Instructor)")
            .Produces<ApiResponse<object>>(200)
            .Produces(404);

        group.MapGet("/wallet/{walletId:guid}", GetTransactionsByWalletAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff, Role.Instructor))
            .WithName("GetTransactionsByWallet")
            .WithDescription("Lấy giao dịch theo ví (Admin/Staff/Instructor, phân trang)")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);

        group.MapGet("/revenue", GetTotalRevenueAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .WithName("GetTotalRevenue")
            .WithDescription("Lấy tổng doanh thu nền tảng trong khoảng thời gian (Admin only)")
            .Produces<ApiResponse<object>>(200)
            .Produces(401);

        return group;
    }

    // ── Handlers ──

    private static async Task<IResult> GetAllTransactionsAsync(
        [FromServices] ITransactionService transactionService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await transactionService.GetAllTransactionsAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetTransactionByIdAsync(
        Guid transactionId,
        [FromServices] ITransactionService transactionService)
    {
        var result = await transactionService.GetTransactionByIdAsync(transactionId);
        return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetTransactionsByWalletAsync(
        Guid walletId,
        [FromServices] ITransactionService transactionService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await transactionService.GetTransactionsByWalletAsync(walletId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetTotalRevenueAsync(
        [FromServices] ITransactionService transactionService,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate >= endDate)
            return Results.BadRequest(ApiResponse<decimal>.FailureResponse("Ngày bắt đầu phải trước ngày kết thúc"));

        var result = await transactionService.GetTotalRevenueAsync(startDate, endDate);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
