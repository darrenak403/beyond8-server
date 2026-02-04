using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Payments;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<ApiResponse<PaymentResponse>> ProcessPaymentAsync(Guid orderId, ProcessPaymentRequest request);
    Task<ApiResponse<bool>> ConfirmPaymentAsync(string transactionId);
    Task<ApiResponse<bool>> RefundPaymentAsync(Guid orderId, RefundPaymentRequest request);
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByOrderAsync(Guid orderId);
    Task<ApiResponse<List<PaymentResponse>>> GetPaymentsByUserAsync(PaginationRequest pagination, Guid userId);
}