using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Usages;
using Beyond8.Integration.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Integration.Api.Apis
{
    public static class AiUsageApis
    {
        public static IEndpointRouteBuilder MapAiUsageApi(this IEndpointRouteBuilder builder)
        {
            builder.MapGroup("/api/v1/ai-usage")
                .MapAiUsageRoutes()
                .WithTags("AI Usage Api")
                .RequireRateLimiting("Fixed");

            return builder;
        }

        private static RouteGroupBuilder MapAiUsageRoutes(this RouteGroupBuilder group)
        {
            group.MapGet("/me", GetMyUsage)
                .WithName("GetMyUsage")
                .WithDescription("Lấy lịch sử sử dụng AI của người dùng hiện tại")
                .RequireAuthorization(r => r.RequireRole(Role.Student, Role.Instructor))
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/all", GetAllUsage)
                .WithName("GetAllUsage")
                .WithDescription("Lấy tất cả records sử dụng AI (Admin only). Query: pageNumber, pageSize, startDate, endDate (optional).")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/user/{userId:guid}", GetUsageByUser)
                .WithName("GetUsageByUser")
                .WithDescription("Lấy lịch sử sử dụng AI của user cụ thể (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<List<AiUsageResponse>>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            group.MapGet("/statistics", GetUsageStatistics)
                .WithName("GetUsageStatistics")
                .WithDescription("Lấy thống kê tổng quan sử dụng AI (Admin only)")
                .RequireAuthorization(r => r.RequireRole(Role.Admin))
                .Produces<ApiResponse<AiUsageStatisticsResponse>>(StatusCodes.Status200OK)
                .Produces<ApiResponse<AiUsageStatisticsResponse>>(StatusCodes.Status400BadRequest)
                .Produces(StatusCodes.Status401Unauthorized);

            return group;
        }

        private static async Task<IResult> GetMyUsage(
            [FromServices] IAiUsageService usageService,
            [FromServices] ICurrentUserService currentUserService,
            [AsParameters] PaginationRequest paginationRequest)
        {
            var result = await usageService.GetUserUsageHistoryAsync(currentUserService.UserId, paginationRequest);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetAllUsage(
            [FromServices] IAiUsageService usageService,
            [AsParameters] AiUsageSearchRequest searchRequest)
        {
            var result = await usageService.GetAllUsageAsync(searchRequest);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetUsageByUser(
            [FromRoute] Guid userId,
            [FromServices] IAiUsageService usageService,
            [AsParameters] PaginationRequest paginationRequest)
        {
            var result = await usageService.GetUserUsageHistoryAsync(userId, paginationRequest);

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }

        private static async Task<IResult> GetUsageStatistics(
            [FromServices] IAiUsageService usageService)
        {
            var result = await usageService.GetUsageStatisticsAsync();

            return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
        }
    }
}
