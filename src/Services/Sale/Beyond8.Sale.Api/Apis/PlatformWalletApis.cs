using Beyond8.Common;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Common.Utilities.Pagination;
using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class PlatformWalletApis
{
    public static IEndpointRouteBuilder MapPlatformWalletApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/platform-wallet")
            .MapPlatformWalletRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Platform Wallet Api");

        return builder;
    }

    public static RouteGroupBuilder MapPlatformWalletRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetPlatformWalletAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .WithName("GetPlatformWallet")
            .WithDescription("Lấy thông tin ví nền tảng (Admin only)")
            .Produces<ApiResponse<PlatformWalletResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/transactions", GetPlatformWalletTransactionsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .WithName("GetPlatformWalletTransactions")
            .WithDescription("Lấy lịch sử giao dịch ví nền tảng (Admin only)")
            .Produces<ApiResponse<List<PlatformWalletTransactionResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetPlatformWalletAsync(
        [FromServices] IPlatformWalletService platformWalletService)
    {
        var result = await platformWalletService.GetPlatformWalletAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetPlatformWalletTransactionsAsync(
        [FromServices] IPlatformWalletService platformWalletService,
        [AsParameters] PaginationRequest pagination)
    {
        var result = await platformWalletService.GetPlatformWalletTransactionsAsync(pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
