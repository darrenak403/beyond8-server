using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Dtos.Carts;
using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Mappings.Carts;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Sale.Application.Services.Implements;

/// <summary>
/// Service for managing shopping cart operations.
/// Delegates order creation to OrderService to avoid duplicate logic.
/// </summary>
public class CartService(
    ILogger<CartService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    IOrderService orderService) : ICartService
{
    public async Task<ApiResponse<CartResponse>> GetCartAsync(Guid userId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);
            return ApiResponse<CartResponse>.SuccessResponse(cart.ToResponse(), "Lấy giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get cart for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<CartResponse>> AddToCartAsync(Guid userId, AddToCartRequest request)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            // Check if course already in cart
            var existingItem = cart.CartItems.FirstOrDefault(ci => ci.CourseId == request.CourseId);
            if (existingItem != null)
                return ApiResponse<CartResponse>.FailureResponse("Khóa học đã có trong giỏ hàng");

            // Get course details from Catalog service
            var courseResult = await catalogClient.GetCourseByIdAsync(request.CourseId);
            if (!courseResult.IsSuccess || courseResult.Data == null)
                return ApiResponse<CartResponse>.FailureResponse("Khóa học không tồn tại");

            var course = courseResult.Data;

            // Create cart item with course snapshot
            var cartItem = new CartItem
            {
                CartId = cart.Id,
                CourseId = course.Id,
                CourseTitle = course.Title,
                CourseThumbnail = course.ThumbnailUrl,
                InstructorId = course.InstructorId,
                InstructorName = course.InstructorName,
                OriginalPrice = course.Price
            };

            // Use repository to add - ensures correct EF Core tracking state
            await unitOfWork.CartItemRepository.AddAsync(cartItem);
            await unitOfWork.SaveChangesAsync();

            // Reload cart with updated items
            var updatedCart = await GetCartByUserIdAsync(userId);

            logger.LogInformation("Course {CourseId} added to cart for user {UserId}", request.CourseId, userId);

            return ApiResponse<CartResponse>.SuccessResponse(updatedCart!.ToResponse(), "Thêm vào giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add course {CourseId} to cart for user {UserId}", request.CourseId, userId);
            throw;
        }
    }

    public async Task<ApiResponse<CartResponse>> RemoveFromCartAsync(Guid userId, Guid courseId)
    {
        try
        {
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CourseId == courseId);
            if (cartItem == null)
                return ApiResponse<CartResponse>.FailureResponse("Khóa học không có trong giỏ hàng");

            cart.CartItems.Remove(cartItem);
            await unitOfWork.CartItemRepository.DeleteAsync(cartItem.Id);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Course {CourseId} removed from cart for user {UserId}", courseId, userId);

            return ApiResponse<CartResponse>.SuccessResponse(cart.ToResponse(), "Xóa khỏi giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to remove course {CourseId} from cart for user {UserId}", courseId, userId);
            throw;
        }
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(Guid userId)
    {
        try
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null)
                return ApiResponse<bool>.SuccessResponse(true, "Giỏ hàng đã trống");

            // Delete all cart items
            foreach (var item in cart.CartItems.ToList())
            {
                await unitOfWork.CartItemRepository.DeleteAsync(item.Id);
            }

            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Cart cleared for user {UserId}", userId);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to clear cart for user {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// Checkout cart items and create order.
    /// This method delegates actual order creation to OrderService to avoid duplicate logic.
    /// Workflow: Validate cart → Select items → Convert to order request → Delegate to OrderService → Clear cart items.
    /// </summary>
    public async Task<ApiResponse<OrderResponse>> CheckoutCartAsync(Guid userId, CheckoutCartRequest request)
    {
        try
        {
            var cart = await GetCartByUserIdAsync(userId);
            if (cart == null || !cart.CartItems.Any())
                return ApiResponse<OrderResponse>.FailureResponse("Giỏ hàng trống, không thể thanh toán");

            // Determine and validate which items to checkout
            var itemsValidation = ValidateAndSelectCartItems(cart, request.SelectedItems);
            if (!itemsValidation.IsValid)
                return ApiResponse<OrderResponse>.FailureResponse(itemsValidation.ErrorMessage!);

            var itemsToCheckout = itemsValidation.SelectedItems;

            // Convert cart items to order request with instructor coupons (separation: cart → order)
            var orderRequest = ConvertCartItemsToOrderRequest(itemsToCheckout, request);

            // Delegate order creation to OrderService (no duplicate logic)
            var orderResult = await orderService.CreateOrderAsync(orderRequest, userId);

            if (orderResult.IsSuccess)
            {
                // Remove only checked out items from cart
                await RemoveCheckedOutItemsFromCartAsync(itemsToCheckout.Select(x => x.CartItem));

                logger.LogInformation(
                    "Cart checked out for user {UserId}, order {OrderId}, {ItemCount} items removed from cart",
                    userId, orderResult.Data!.Id, itemsToCheckout.Count);
            }

            return orderResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to checkout cart for user {UserId}", userId);
            throw;
        }
    }

    // ══════════════════════════════════════════════════════════════════
    // Helper Methods
    // ══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Validate and select cart items for checkout with instructor coupons.
    /// Returns selected items with their corresponding instructor coupons or error message.
    /// </summary>
    private (bool IsValid, string? ErrorMessage, List<(CartItem CartItem, string? InstructorCouponCode)> SelectedItems)
        ValidateAndSelectCartItems(Cart cart, List<CheckoutItemRequest> selectedItems)
    {
        // Case 1: No selection → checkout all items without instructor coupons
        if (!selectedItems.Any())
        {
            var allItems = cart.CartItems.Select(ci => (ci, (string?)null)).ToList();
            return (true, null, allItems);
        }

        // Case 2: Validate selected items exist in cart
        var cartCourseIds = cart.CartItems.Select(ci => ci.CourseId).ToHashSet();
        var requestedCourseIds = selectedItems.Select(si => si.CourseId).ToList();
        var missingCourseIds = requestedCourseIds.Except(cartCourseIds).ToList();

        if (missingCourseIds.Any())
            return (false, $"Các khóa học sau không có trong giỏ hàng: {string.Join(", ", missingCourseIds)}", []);

        // Case 3: Select requested items with instructor coupons
        var couponMapping = selectedItems.ToDictionary(si => si.CourseId, si => si.InstructorCouponCode);
        var itemsWithCoupons = cart.CartItems
            .Where(ci => couponMapping.ContainsKey(ci.CourseId))
            .Select(ci => (ci, couponMapping[ci.CourseId]))
            .ToList();

        if (!itemsWithCoupons.Any())
            return (false, "Không tìm thấy khóa học được chọn trong giỏ hàng", []);

        return (true, null, itemsWithCoupons);
    }

    /// <summary>
    /// Convert cart items to order request with instructor coupons.
    /// This is the boundary between Cart domain and Order domain.
    /// Maps instructor coupon codes from checkout request to order items.
    /// </summary>
    private static CreateOrderRequest ConvertCartItemsToOrderRequest(
        List<(CartItem CartItem, string? InstructorCouponCode)> itemsWithCoupons,
        CheckoutCartRequest request)
    {
        return new CreateOrderRequest
        {
            Items = itemsWithCoupons.Select(item => new OrderItemRequest
            {
                CourseId = item.CartItem.CourseId,
                InstructorCouponCode = item.InstructorCouponCode  // Map instructor coupon per item
            }).ToList(),
            CouponCode = request.CouponCode,  // System coupon for entire order
            Notes = request.Notes
        };
    }

    /// <summary>
    /// Remove checked out items from cart after successful order creation.
    /// </summary>
    private async Task RemoveCheckedOutItemsFromCartAsync(IEnumerable<CartItem> items)
    {
        foreach (var item in items)
        {
            await unitOfWork.CartItemRepository.DeleteAsync(item.Id);
        }
        await unitOfWork.SaveChangesAsync();
    }

    private async Task<Cart?> GetCartByUserIdAsync(Guid userId)
    {
        return await unitOfWork.CartRepository.AsQueryable()
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    private async Task<Cart> GetOrCreateCartAsync(Guid userId)
    {
        var cart = await GetCartByUserIdAsync(userId);
        if (cart != null)
            return cart;

        // Create new cart for the user
        cart = new Cart { UserId = userId };
        await unitOfWork.CartRepository.AddAsync(cart);
        await unitOfWork.SaveChangesAsync();

        return cart;
    }

    public async Task<ApiResponse<int>> CountCartItemsAsync(Guid userId)
    {
        try
        {
            var cart = await GetCartByUserIdAsync(userId);
            int itemCount = cart?.CartItems.Count ?? 0;
            return ApiResponse<int>.SuccessResponse(itemCount, "Đếm số lượng mục trong giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to count cart items for user {UserId}", userId);
            throw;
        }
    }

    public async Task<ApiResponse<Dictionary<Guid, bool>>> CheckCoursesInCartAsync(Guid userId, List<Guid> courseIds)
    {
        try
        {
            if (!courseIds.Any())
                return ApiResponse<Dictionary<Guid, bool>>.SuccessResponse(
                    new Dictionary<Guid, bool>(),
                    "Danh sách khóa học rỗng");

            var cartCourseIds = await unitOfWork.CartItemRepository.AsQueryable()
                .Where(ci => ci.Cart.UserId == userId && courseIds.Contains(ci.CourseId))
                .Select(ci => ci.CourseId)
                .ToListAsync();

            // Build result dictionary
            var result = courseIds.ToDictionary(
                courseId => courseId,
                courseId => cartCourseIds.Contains(courseId));

            logger.LogInformation(
                "Checked {TotalCourses} courses for user {UserId}, {InCartCount} in cart",
                courseIds.Count, userId, cartCourseIds.Count);

            return ApiResponse<Dictionary<Guid, bool>>.SuccessResponse(
                result,
                "Kiểm tra khóa học trong giỏ hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check courses in cart for user {UserId}", userId);
            throw;
        }
    }
}
