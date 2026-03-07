using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IPaymentService
{
    // Run cleanup of expired payments (for Hangfire recurring job)
    Task RunCleanupAsync();

    // ── Student (Authenticated) ──
    Task<ApiResponse<PaymentUrlResponse>> ProcessPaymentAsync(Guid orderId, string returnUrl, string ipAddress);
    Task<ApiResponse<PaymentUrlResponse>> ProcessSubscriptionAsync(string planCode, Guid userId, string returnUrl, string ipAddress);
    Task<ApiResponse<PendingPaymentResponse>> CheckPendingPaymentAsync(PaymentPurpose purpose, Guid userId);
    Task<ApiResponse<bool>> ConfirmPaymentAsync(string transactionId);
    Task<ApiResponse<PaymentResponse>> CheckPaymentStatusAsync(Guid paymentId);
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByUserAsync(PaginationRequest pagination, Guid userId);

    // ── Instructor: Wallet Top-Up ──
    Task<ApiResponse<PaymentUrlResponse>> ProcessTopUpAsync(Guid instructorId, decimal amount, string returnUrl, string ipAddress);

    // ── System (VNPay webhook — AllowAnonymous + HMAC verification) ──
    Task<ApiResponse<bool>> HandleVNPayCallbackAsync(VNPayCallbackRequest request, string rawQueryString);

    // ── Owner / Admin ──
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByOrderAsync(Guid orderId);
}