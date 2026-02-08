using Beyond8.Common;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface IOrderService
{
    // ── Student (Authenticated) ──
    Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, Guid userId);

    // ── Owner / Admin ──
    Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserAsync(PaginationRequest pagination, Guid userId);

    // ── Admin Only ──
    Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request);
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByStatusAsync(OrderStatus status, PaginationRequest pagination);

    // ── Instructor ──
    Task<ApiResponse<List<OrderResponse>>> GetOrdersByInstructorAsync(Guid instructorId, PaginationRequest pagination);
}