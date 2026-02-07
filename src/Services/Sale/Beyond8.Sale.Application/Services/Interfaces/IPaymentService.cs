using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payments;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentUrlResponse>> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequest request, string returnUrl, string ipAddress);
    Task<ApiResponse<bool>> HandleVNPayCallbackAsync(VNPayCallbackRequest request, string rawQueryString);
    Task<ApiResponse<bool>> ConfirmPaymentAsync(string transactionId);
    Task<ApiResponse<PaymentResponse>> CheckPaymentStatusAsync(Guid paymentId);
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByOrderAsync(Guid orderId);
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByUserAsync(PaginationRequest pagination, Guid userId);
}