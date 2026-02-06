using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, Guid userId);
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId);
    Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserAsync(PaginationRequest pagination, Guid userId);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByInstructorAsync(Guid instructorId, PaginationRequest pagination);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByStatusAsync(OrderStatus status, PaginationRequest pagination);
    Task<ApiResponse<OrderStatisticsResponse>> GetOrderStatisticsAsync(Guid? instructorId = null);
}