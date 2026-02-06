using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class OrderApis
{
    public static IEndpointRouteBuilder MapOrderApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/orders")
            .MapOrderRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Order Api");

        return builder;
    }

    public static RouteGroupBuilder MapOrderRoutes(this RouteGroupBuilder group)
    {
        // Customer Operations
        group.MapPost("/", CreateOrderAsync)
            .WithName("CreateOrder")
            .WithDescription("Tạo đơn hàng mới (Authenticated user only)")
            .RequireAuthorization()
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/{orderId}", GetOrderByIdAsync)
            .WithName("GetOrderById")
            .WithDescription("Lấy thông tin đơn hàng theo ID (Order owner or Admin)")
            .RequireAuthorization()
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/user/{userId}", GetOrdersByUserAsync)
            .WithName("GetOrdersByUser")
            .WithDescription("Lấy danh sách đơn hàng của người dùng (User or Admin, paginated)")
            .RequireAuthorization()
            .Produces<ApiResponse<List<OrderResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPost("/{orderId}/cancel", CancelOrderAsync)
            .WithName("CancelOrder")
            .WithDescription("Hủy đơn hàng (Order owner or Admin)")
            .RequireAuthorization()
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<bool>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Instructor Operations
        group.MapGet("/instructor/{instructorId}", GetOrdersByInstructorAsync)
            .WithName("GetOrdersByInstructor")
            .WithDescription("Lấy danh sách đơn hàng bán khóa học của instructor (Instructor - own data or Admin, paginated)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor, Role.Admin))
            .Produces<ApiResponse<List<OrderResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        // Admin Operations
        group.MapGet("/status/{status}", GetOrdersByStatusAsync)
            .WithName("GetOrdersByStatus")
            .WithDescription("Lấy danh sách đơn hàng theo trạng thái (Admin only, paginated)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<List<OrderResponse>>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapPatch("/{orderId}/status", UpdateOrderStatusAsync)
            .WithName("UpdateOrderStatus")
            .WithDescription("Cập nhật trạng thái đơn hàng (Admin only)")
            .RequireAuthorization(x => x.RequireRole(Role.Admin))
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        group.MapGet("/statistics", GetOrderStatisticsAsync)
            .WithName("GetOrderStatistics")
            .WithDescription("Lấy thống kê đơn hàng (Instructor stats or Admin for all)")
            .RequireAuthorization(x => x.RequireRole(Role.Instructor, Role.Admin))
            .Produces<ApiResponse<OrderStatisticsResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> CreateOrderAsync(
        [FromBody] CreateOrderRequest request,
        [FromServices] IOrderService orderService,
        [FromServices] IValidator<CreateOrderRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        // Validate request data
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await orderService.CreateOrderAsync(request, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetOrderByIdAsync(
        [FromRoute] Guid orderId,
        [FromServices] IOrderService orderService,
        [FromServices] ICurrentUserService currentUserService)
    {
        // First get the order to check ownership
        var orderResult = await orderService.GetOrderByIdAsync(orderId);
        if (!orderResult.IsSuccess)
            return Results.NotFound(orderResult);

        // Validate authorization: must be order owner or admin
        if (orderResult.Data!.UserId != currentUserService.UserId && !currentUserService.IsInRole(Role.Admin))
            return Results.Forbid();

        return Results.Ok(orderResult);
    }

    private static async Task<IResult> GetOrdersByUserAsync(
        [FromRoute] Guid userId,
        [AsParameters] PaginationRequest pagination,
        [FromServices] IOrderService orderService,
        [FromServices] ICurrentUserService currentUserService)
    {
        // Validate authorization: must be requesting own data or admin
        if (userId != currentUserService.UserId && !currentUserService.IsInRole(Role.Admin))
            return Results.Forbid();

        var result = await orderService.GetOrdersByUserAsync(pagination, userId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CancelOrderAsync(
        [FromRoute] Guid orderId,
        [FromServices] IOrderService orderService,
        [FromServices] ICurrentUserService currentUserService)
    {
        // First get the order to check ownership
        var orderResult = await orderService.GetOrderByIdAsync(orderId);
        if (!orderResult.IsSuccess)
            return Results.NotFound(orderResult);

        // Validate authorization: must be order owner or admin
        if (orderResult.Data!.UserId != currentUserService.UserId && !currentUserService.IsInRole(Role.Admin))
            return Results.Forbid();

        var result = await orderService.CancelOrderAsync(orderId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetOrdersByInstructorAsync(
        [FromRoute] Guid instructorId,
        [AsParameters] PaginationRequest pagination,
        [FromServices] IOrderService orderService,
        [FromServices] ICurrentUserService currentUserService)
    {
        // Validate authorization: must be requesting own data or admin
        if (instructorId != currentUserService.UserId && !currentUserService.IsInRole(Role.Admin))
            return Results.Forbid();

        var result = await orderService.GetOrdersByInstructorAsync(instructorId, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetOrdersByStatusAsync(
        [FromRoute] OrderStatus status,
        [AsParameters] PaginationRequest pagination,
        [FromServices] IOrderService orderService)
    {
        var result = await orderService.GetOrdersByStatusAsync(status, pagination);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> UpdateOrderStatusAsync(
        [FromRoute] Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        [FromServices] IOrderService orderService)
    {
        var result = await orderService.UpdateOrderStatusAsync(orderId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetOrderStatisticsAsync(
        [FromQuery] Guid? instructorId = null,
        [FromServices] IOrderService orderService = null!,
        [FromServices] ICurrentUserService currentUserService = null!)
    {
        // Validate authorization
        if (instructorId.HasValue)
        {
            // If requesting specific instructor stats: must be that instructor or admin
            if (instructorId != currentUserService.UserId && !currentUserService.IsInRole(Role.Admin))
                return Results.Forbid();
        }
        else
        {
            // If requesting all stats: admin only
            if (!currentUserService.IsInRole(Role.Admin))
                return Results.Forbid();
        }

        var result = await orderService.GetOrderStatisticsAsync(instructorId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}