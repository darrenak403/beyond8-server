using Beyond8.Common;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Settlements;
using Beyond8.Sale.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class SettlementApis
{
    public static IEndpointRouteBuilder MapSettlementApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/settlements")
            .MapSettlementRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Settlement Api");

        return builder;
    }

    public static RouteGroupBuilder MapSettlementRoutes(this RouteGroupBuilder group)
    {
        // Admin: trigger processing now (for demo / maintenance)
        group.MapPost("/trigger", TriggerProcessPendingSettlementsAsync)
            .WithName("TriggerProcessPendingSettlements")
            .WithDescription("Trigger settlement processing immediately (Admin only). Use for demo/maintenance only.")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/upcoming", GetUpcomingByOrderAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Admin, Role.Staff))
            .WithName("GetUpcomingByOrder")
            .WithDescription("Get upcoming settlements grouped by order (instructor + platform amounts)")
            .Produces<ApiResponse<List<UpcomingByOrderResponse>>>(200)
            .Produces(401);

        group.MapGet("/my-upcoming", GetMyUpcomingSettlementsAsync)
            .RequireAuthorization(x => x.RequireRole(Role.Instructor))
            .WithName("GetMyUpcomingSettlements")
            .WithDescription("Get upcoming settlements for current instructor (their incoming releases)")
            .Produces<ApiResponse<List<UpcomingSettlementResponse>>>(200)
            .Produces(401);

        return group;
    }

    private static async Task<IResult> TriggerProcessPendingSettlementsAsync(
        [FromServices] ISettlementService settlementService)
    {
        var result = await settlementService.ProcessPendingSettlementsAsync();
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetUpcomingByOrderAsync(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [AsParameters] PaginationRequest pagination,
        [FromServices] ISettlementService settlementService)
    {
        var result = await settlementService.GetUpcomingByOrderAsync(from, to, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyUpcomingSettlementsAsync(
        [AsParameters] PaginationRequest pagination,
        [FromServices] ISettlementService settlementService,
        [FromServices] ICurrentUserService currentUser)
    {
        var userId = currentUser.UserId;
        var result = await settlementService.GetMyUpcomingSettlementsAsync(userId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
