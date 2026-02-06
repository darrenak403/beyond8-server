using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Carts;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Beyond8.Sale.Api.Apis;

public static class CartApis
{
    public static IEndpointRouteBuilder MapCartApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/cart")
            .MapCartRoutes()
            .RequireRateLimiting("Fixed")
            .RequireAuthorization()
            .WithTags("Cart Api");

        return builder;
    }

    public static RouteGroupBuilder MapCartRoutes(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCartAsync)
            .WithName("GetCart")
            .WithDescription("Lấy giỏ hàng hiện tại")
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK);

        group.MapPost("/add", AddToCartAsync)
            .WithName("AddToCart")
            .WithDescription("Thêm khóa học vào giỏ hàng")
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status400BadRequest);

        group.MapDelete("/remove/{courseId}", RemoveFromCartAsync)
            .WithName("RemoveFromCart")
            .WithDescription("Xóa khóa học khỏi giỏ hàng")
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<CartResponse>>(StatusCodes.Status400BadRequest);

        group.MapDelete("/clear", ClearCartAsync)
            .WithName("ClearCart")
            .WithDescription("Xóa toàn bộ giỏ hàng")
            .Produces<ApiResponse<bool>>(StatusCodes.Status200OK);

        group.MapPost("/checkout", CheckoutCartAsync)
            .WithName("CheckoutCart")
            .WithDescription("Thanh toán giỏ hàng — tạo đơn hàng từ giỏ")
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<OrderResponse>>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static async Task<IResult> GetCartAsync(
        [FromServices] ICartService cartService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await cartService.GetCartAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> AddToCartAsync(
        [FromBody] AddToCartRequest request,
        [FromServices] ICartService cartService,
        [FromServices] IValidator<AddToCartRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await cartService.AddToCartAsync(currentUserService.UserId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> RemoveFromCartAsync(
        [FromRoute] Guid courseId,
        [FromServices] ICartService cartService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await cartService.RemoveFromCartAsync(currentUserService.UserId, courseId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> ClearCartAsync(
        [FromServices] ICartService cartService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await cartService.ClearCartAsync(currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> CheckoutCartAsync(
        [FromBody] CheckoutCartRequest request,
        [FromServices] ICartService cartService,
        [FromServices] IValidator<CheckoutCartRequest> validator,
        [FromServices] ICurrentUserService currentUserService)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var result = await cartService.CheckoutCartAsync(currentUserService.UserId, request);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
