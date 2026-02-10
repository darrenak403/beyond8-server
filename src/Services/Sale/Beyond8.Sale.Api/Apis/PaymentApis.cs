using Beyond8.Common;
using Beyond8.Common.Extensions;
using Beyond8.Common.Security;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Application.Helpers;
using Beyond8.Sale.Application.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Beyond8.Sale.Api.Apis;

public static class PaymentApis
{
    public static IEndpointRouteBuilder MapPaymentApi(this IEndpointRouteBuilder builder)
    {
        builder.MapGroup("/api/v1/payments")
            .MapPaymentRoutes()
            .RequireRateLimiting("Fixed")
            .WithTags("Payment Api");

        return builder;
    }

    public static RouteGroupBuilder MapPaymentRoutes(this RouteGroupBuilder group)
    {
        group.MapPost("/process", ProcessPaymentAsync)
            .WithName("ProcessPayment")
            .WithDescription("Khởi tạo thanh toán VNPay cho đơn hàng (Student/Authenticated)")
            .RequireAuthorization()
            .Produces<ApiResponse<PaymentUrlResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PaymentUrlResponse>>(StatusCodes.Status400BadRequest);

        group.MapGet("/vnpay/callback", HandleVNPayCallbackAsync)
            .WithName("VNPayCallback")
            .WithDescription("VNPay callback — xử lý kết quả thanh toán (Public - VNPay webhook)")
            .AllowAnonymous()
            .Produces<string>(StatusCodes.Status200OK);

        group.MapGet("/{paymentId}/status", CheckPaymentStatusAsync)
            .WithName("CheckPaymentStatus")
            .WithDescription("Kiểm tra trạng thái thanh toán (Student/Authenticated)")
            .RequireAuthorization()
            .Produces<ApiResponse<PaymentResponse>>(StatusCodes.Status200OK)
            .Produces<ApiResponse<PaymentResponse>>(StatusCodes.Status404NotFound);

        group.MapGet("/order/{orderId}", GetPaymentsByOrderAsync)
            .WithName("GetPaymentsByOrder")
            .WithDescription("Lấy danh sách thanh toán theo đơn hàng (Order Owner or Admin)")
            .RequireAuthorization()
            .Produces<ApiResponse<List<PaymentResponse>>>(StatusCodes.Status200OK);

        group.MapGet("/my-payments", GetMyPaymentsAsync)
            .WithName("GetMyPayments")
            .WithDescription("Lấy lịch sử thanh toán của user (Student/Authenticated, paginated)")
            .RequireAuthorization()
            .Produces<ApiResponse<List<PaymentResponse>>>(StatusCodes.Status200OK);

        return group;
    }

    private static async Task<IResult> ProcessPaymentAsync(
        [FromBody] ProcessPaymentRequest request,
        [FromServices] IPaymentService paymentService,
        [FromServices] IValidator<ProcessPaymentRequest> validator,
        [FromServices] ICurrentUserService currentUserService,
        [FromServices] IOptions<VNPaySettings> vnPayOptions,
        HttpContext httpContext)
    {
        if (!request.ValidateRequest(validator, out var validationResult))
            return validationResult!;

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
        var callbackUrl = $"{vnPayOptions.Value.BackendCallbackUrl.TrimEnd('/')}/api/v1/payments/vnpay/callback";

        var result = await paymentService.ProcessPaymentAsync(request.OrderId, callbackUrl, ipAddress);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    /// <summary>
    /// VNPay redirects user here after payment. We validate the signature,
    /// update payment/order status, then redirect to frontend with result.
    /// </summary>
    private static async Task<IResult> HandleVNPayCallbackAsync(
        [FromServices] IPaymentService paymentService,
        [FromServices] IOptions<VNPaySettings> vnPayOptions,
        ILogger<VNPaySettings> logger,
        HttpContext httpContext)
    {
        var rawQueryString = httpContext.Request.QueryString.Value ?? "";

        logger.LogInformation("VNPay callback received — QueryString: {QueryString}", rawQueryString);

        var query = httpContext.Request.Query;
        var callbackRequest = new VNPayCallbackRequest
        {
            vnp_TmnCode = query["vnp_TmnCode"].ToString(),
            vnp_Amount = query["vnp_Amount"].ToString(),
            vnp_BankCode = query["vnp_BankCode"].ToString(),
            vnp_BankTranNo = query["vnp_BankTranNo"].ToString(),
            vnp_CardType = query["vnp_CardType"].ToString(),
            vnp_PayDate = query["vnp_PayDate"].ToString(),
            vnp_OrderInfo = query["vnp_OrderInfo"].ToString(),
            vnp_TransactionNo = query["vnp_TransactionNo"].ToString(),
            vnp_ResponseCode = query["vnp_ResponseCode"].ToString(),
            vnp_TransactionStatus = query["vnp_TransactionStatus"].ToString(),
            vnp_TxnRef = query["vnp_TxnRef"].ToString(),
            vnp_SecureHashType = query["vnp_SecureHashType"].ToString(),
            vnp_SecureHash = query["vnp_SecureHash"].ToString()
        };

        var result = await paymentService.HandleVNPayCallbackAsync(callbackRequest, rawQueryString);

        var frontendUrl = vnPayOptions.Value.ReturnUrl;
        var paymentStatus = result.IsSuccess ? "success" : "failed";
        return Results.Redirect($"{frontendUrl}{rawQueryString}&payment_status={paymentStatus}");
    }

    private static async Task<IResult> CheckPaymentStatusAsync(
        [FromRoute] Guid paymentId,
        [FromServices] IPaymentService paymentService)
    {
        var result = await paymentService.CheckPaymentStatusAsync(paymentId);
        return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
    }

    private static async Task<IResult> GetPaymentsByOrderAsync(
        [FromRoute] Guid orderId,
        [FromServices] IPaymentService paymentService)
    {
        var result = await paymentService.GetPaymentsByOrderAsync(orderId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }

    private static async Task<IResult> GetMyPaymentsAsync(
        [AsParameters] PaginationRequest pagination,
        [FromServices] IPaymentService paymentService,
        [FromServices] ICurrentUserService currentUserService)
    {
        var result = await paymentService.GetPaymentsByUserAsync(pagination, currentUserService.UserId);
        return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
    }
}
