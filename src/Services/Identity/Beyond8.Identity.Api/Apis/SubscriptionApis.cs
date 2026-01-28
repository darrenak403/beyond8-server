using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Identity.Application.Dtos.Users;
using Beyond8.Identity.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Identity.Api.Apis;

public static class SubscriptionApis
{
    public static IEndpointRouteBuilder MapSubscriptionApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/subscriptions")
            .MapSubscriptionRoutes()
            .WithTags("Subscription Api")
            .RequireRateLimiting("Fixed");

        return builder;
    }

    public static RouteGroupBuilder MapSubscriptionRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/me", GetMySubscriptionStatsAsync)
            .WithName("GetMySubscriptionStats")
            .WithDescription("Lấy thông tin gói đăng ký của người dùng hiện tại")
            .RequireAuthorization()
            .Produces<ApiResponse<SubscriptionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SubscriptionResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/plans", GetSubscriptionPlansAsync)
            .WithName("GetSubscriptionPlans")
            .WithDescription("Lấy danh sách gói đăng ký")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<List<SubscriptionPlanResponse>>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<List<SubscriptionPlanResponse>>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateSubscriptionAsync)
            .WithName("UpdateSubscription")
            .WithDescription("Cập nhật gói đăng ký của người dùng")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<SubscriptionResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<SubscriptionResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> GetMySubscriptionStatsAsync(
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] ISubscriptionService subscriptionService)
    {
        var response = await subscriptionService.GetMySubscriptionStatsAsync(currentUserService.UserId);
        return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> GetSubscriptionPlansAsync(
        [FromServices] ISubscriptionService subscriptionService)
    {
        var response = await subscriptionService.GetSubscriptionPlansAsync();
        return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
    }

    private static async Task<IResult> UpdateSubscriptionAsync(
        [FromBody] UpdateSubscriptionRequest request,
        [FromRoute] Guid id,
        [FromServices] ISubscriptionService subscriptionService)
    {
        var response = await subscriptionService.UpdateSubscriptionAsync(id, request);
        return response.IsSuccess ? Results.Ok(response) : Results.BadRequest(response);
    }
}
