using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Dtos.Carts;
using Beyond8.Sale.Application.Dtos.Orders;

namespace Beyond8.Sale.Application.Services.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartResponse>> GetCartAsync(Guid userId);
    Task<ApiResponse<CartResponse>> AddToCartAsync(Guid userId, AddToCartRequest request);
    Task<ApiResponse<CartResponse>> RemoveFromCartAsync(Guid userId, Guid courseId);
    Task<ApiResponse<bool>> ClearCartAsync(Guid userId);
    Task<ApiResponse<OrderResponse>> CheckoutCartAsync(Guid userId, CheckoutCartRequest request);
    Task<ApiResponse<int>> CountCartItemsAsync(Guid userId);
    Task<ApiResponse<Dictionary<Guid, bool>>> CheckCoursesInCartAsync(Guid userId, List<Guid> courseIds);
}
