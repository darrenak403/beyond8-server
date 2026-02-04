using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request);
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId);
    Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserAsync(PaginationRequest pagination, Guid userId);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByInstructorAsync(Guid instructorId);
}