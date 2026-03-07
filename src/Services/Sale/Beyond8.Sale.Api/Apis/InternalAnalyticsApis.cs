using Beyond8.Common.Security;
using Beyond8.Sale.Application.Services.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Beyond8.Sale.Api.Apis;

public static class InternalAnalyticsApis
{
    public static IEndpointRouteBuilder MapInternalAnalyticsApi(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/internal/orders")
            .WithTags("Internal - Analytics")
            .AllowAnonymous();

        group.MapGet("/revenue-by-date", GetRevenueByDate)
            .WithName("GetRevenueByDate")
            .Produces<List<Beyond8.Sale.Application.Dtos.Analytics.DailyRevenueSummary>>();

        var walletGroup = app.MapGroup("/api/v1/internal/wallets")
            .WithTags("Internal - Analytics")
            .AllowAnonymous();

        walletGroup.MapGet("/instructors/{instructorId:guid}", GetInstructorWallet)
            .WithName("GetInstructorWalletInternal")
            .WithSummary("Get instructor wallet stats for analytics");

        return app;
    }

    private static async Task<IResult> GetRevenueByDate(
        [FromServices] IOrderService orderService,
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        if (from > to)
            return Results.BadRequest("'from' phải nhỏ hơn hoặc bằng 'to'");

        if ((to - from).TotalDays > 366)
            return Results.BadRequest("Khoảng thời gian tối đa là 366 ngày");

        var result = await orderService.GetRevenueByDateRangeAsync(from, to);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetInstructorWallet(
        Guid instructorId,
        [FromServices] IInstructorWalletService walletService)
    {
        var result = await walletService.GetWalletByInstructorAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.StatusCode(500);
    }
}
