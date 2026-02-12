using Beyond8.Common;
using Beyond8.Common.Events;
using Beyond8.Common.Events.Cache;
using Beyond8.Common.Events.Sale;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Application.Mappings.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static Beyond8.Sale.Application.Mappings.Orders.OrderMappings;

namespace Beyond8.Sale.Application.Services.Implements;

public class OrderService(
    ILogger<OrderService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    ICouponService couponService,
    ICouponUsageService couponUsageService,
    IPublishEndpoint publishEndpoint) : IOrderService
{
    /// <summary>
    /// Buy Now - Direct single course purchase.
    /// Converts to CreateOrderRequest and delegates to CreateOrderAsync.
    /// </summary>
    public async Task<ApiResponse<OrderResponse>> BuyNowAsync(BuyNowRequest request, Guid userId)
    {
        // Convert Buy Now request to standard order creation request
        var orderRequest = new CreateOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new()
                {
                    CourseId = request.CourseId,
                    InstructorCouponCode = request.InstructorCouponCode
                }
            },
            CouponCode = request.CouponCode,
            Notes = request.Notes
        };

        // Delegate to standard order creation (reuse logic)
        var orderResult = await CreateOrderAsync(orderRequest, userId);

        // Remove from cart only if free course (auto-paid)
        // Paid courses will have cart items removed after payment success in PaymentService
        if (orderResult.IsSuccess && orderResult.Data!.Status == OrderStatus.Paid)
        {
            try
            {
                var cart = await unitOfWork.CartRepository.AsQueryable()
                    .Include(c => c.CartItems)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart != null)
                {
                    var cartItem = cart.CartItems.FirstOrDefault(ci => ci.CourseId == request.CourseId);
                    if (cartItem != null)
                    {
                        await unitOfWork.CartItemRepository.DeleteAsync(cartItem.Id);
                        await unitOfWork.SaveChangesAsync();

                        logger.LogInformation("Removed purchased course {CourseId} from cart for user {UserId}",
                            request.CourseId, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clear cart after Buy Now for user {UserId}, course {CourseId}",
                    userId, request.CourseId);
            }
        }

        return orderResult;
    }

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, Guid userId)
    {
        // Check if user already purchased any of these courses
        var requestedCourseIds = request.Items.Select(i => i.CourseId).ToList();
        
        var alreadyPurchasedCourseIds = await unitOfWork.OrderRepository.AsQueryable()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
            .SelectMany(o => o.OrderItems.Select(oi => oi.CourseId))
            .Where(courseId => requestedCourseIds.Contains(courseId))
            .Distinct()
            .ToListAsync();

        if (alreadyPurchasedCourseIds.Any())
        {
            logger.LogWarning("User {UserId} attempted to purchase already owned courses: {CourseIds}",
                userId, string.Join(", ", alreadyPurchasedCourseIds));
            return ApiResponse<OrderResponse>.FailureResponse(
                "Bạn đã mua một hoặc nhiều khóa học trong đơn hàng này rồi");
        }

        // Validate and calculate totals
        var calculation = await CalculateOrderTotalsAsync(request.Items, request.CouponCode, userId);
        if (!calculation.IsValid)
            return ApiResponse<OrderResponse>.FailureResponse(calculation.ErrorMessage ?? "Lỗi tạo đơn hàng");

        // Create order with mapping
        var orderNumber = GenerateOrderNumber();
        var order = request.ToEntity(orderNumber, calculation.SubTotal, calculation.TotalAmount, calculation.DiscountAmount, calculation.CouponId, userId);

        // Add order items with snapshot using mapping
        foreach (var item in calculation.Items)
        {
            order.OrderItems.Add(item.ToEntity(order.Id));
        }

        await unitOfWork.OrderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order created: {OrderId} for user {UserId}", order.Id, userId);

        // ── Record coupon usage for free orders (Bug fix #1) ──
        if (order.CouponId.HasValue)
        {
            await couponUsageService.RecordUsageAsync(new Dtos.CouponUsages.CreateCouponUsageRequest
            {
                CouponId = order.CouponId.Value,
                UserId = userId,
                OrderId = order.Id,
                DiscountApplied = order.DiscountAmount
            });
        }

        // Publish event for free courses (BR-04)
        if (order.Status == OrderStatus.Paid)
        {
            await publishEndpoint.Publish(new OrderCompletedEvent(
                order.Id,
                userId,
                request.Items.Select(i => i.CourseId).ToList()));

            // Invalidate excluded courses cache so GetAllCourses hides purchased courses immediately
            await publishEndpoint.Publish(new CacheInvalidateEvent($"excluded_courses:{userId}"));

            logger.LogInformation("Free enrollment event published for order {OrderId}", order.Id);
        }

        return ApiResponse<OrderResponse>.SuccessResponse(order.ToResponse(), "Đơn hàng đã được tạo thành công");
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return ApiResponse<OrderResponse>.FailureResponse("Đơn hàng không tồn tại");

        return ApiResponse<OrderResponse>.SuccessResponse(order.ToResponse(), "Lấy thông tin đơn hàng thành công");
    }

    public async Task<ApiResponse<OrderResponse>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return ApiResponse<OrderResponse>.FailureResponse("Đơn hàng không tồn tại");

        order.Status = request.Status;
        order.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order status updated: {OrderId} to {Status}", orderId, request.Status);

        return ApiResponse<OrderResponse>.SuccessResponse(order.ToResponse(), "Cập nhật trạng thái thành công");
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetOrdersByUserAsync(PaginationRequest pagination, Guid userId)
    {
        var orders = await unitOfWork.OrderRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: o => o.UserId == userId,
            orderBy: query => query.OrderByDescending(o => o.CreatedAt),
            includes: query => query.Include(o => o.OrderItems));

        var responses = orders.Items.Select(o => o.ToResponse()).ToList();

        return ApiResponse<List<OrderResponse>>.SuccessPagedResponse(
            responses, orders.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách đơn hàng thành công");
    }

    public async Task<ApiResponse<List<Guid>>> GetPurchasedCourseIdsAsync(Guid userId)
    {
        var purchasedCourseIds = await unitOfWork.OrderRepository.AsQueryable()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
            .SelectMany(o => o.OrderItems.Select(oi => oi.CourseId))
            .Distinct()
            .ToListAsync();

        return ApiResponse<List<Guid>>.SuccessResponse(
            purchasedCourseIds,
            "Lấy danh sách ID khóa học đã mua thành công");
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetOrdersByInstructorAsync(Guid instructorId, PaginationRequest pagination)
    {
        var orders = await unitOfWork.OrderRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: o => o.OrderItems.Any(oi => oi.InstructorId == instructorId),
            orderBy: query => query.OrderByDescending(o => o.CreatedAt),
            includes: query => query.Include(o => o.OrderItems));

        var responses = orders.Items.Select(o => o.ToResponse()).ToList();

        return ApiResponse<List<OrderResponse>>.SuccessPagedResponse(
            responses, orders.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách đơn hàng theo instructor thành công");
    }

    public async Task<ApiResponse<List<OrderResponse>>> GetOrdersByStatusAsync(OrderStatus status, PaginationRequest pagination)
    {
        var orders = await unitOfWork.OrderRepository.GetPagedAsync(
            pageNumber: pagination.PageNumber,
            pageSize: pagination.PageSize,
            filter: o => o.Status == status,
            orderBy: query => query.OrderByDescending(o => o.CreatedAt),
            includes: query => query.Include(o => o.OrderItems));

        var responses = orders.Items.Select(o => o.ToResponse()).ToList();

        return ApiResponse<List<OrderResponse>>.SuccessPagedResponse(
            responses, orders.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách đơn hàng theo trạng thái thành công");
    }

    // Helper methods
    private async Task<(bool IsValid, string? ErrorMessage, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, Guid? CouponId, List<OrderItemSnapshot> Items)> CalculateOrderTotalsAsync(List<OrderItemRequest> items, string? systemCouponCode, Guid userId)
    {
        var orderItems = new List<OrderItemSnapshot>();
        decimal subTotal = 0;
        decimal totalInstructorDiscount = 0;

        // ── Phase 1: Calculate base prices and apply instructor coupons per item ──
        foreach (var item in items)
        {
            var courseResult = await catalogClient.GetCourseByIdAsync(item.CourseId);
            if (!courseResult.IsSuccess)
                return (false, $"Khóa học không tồn tại", 0, 0, 0, null, orderItems);

            if (courseResult.Data == null)
                return (false, "Dữ liệu khóa học không hợp lệ", 0, 0, 0, null, orderItems);

            var course = courseResult.Data;
            var unitPrice = course.FinalPrice; // Use final price (after instructor discounts)
            var itemSubTotal = unitPrice; // Base price before any additional discounts

            // ── Apply instructor coupon for this specific item ──
            decimal itemInstructorDiscount = 0;
            if (!string.IsNullOrEmpty(item.InstructorCouponCode))
            {
                var instructorCouponResult = await couponService.ValidateAndApplyCouponAsync(
                    item.InstructorCouponCode, itemSubTotal, new List<Guid> { item.CourseId }, userId);

                if (!instructorCouponResult.IsSuccess)
                    return (false, instructorCouponResult.Message ?? $"Lỗi xác thực coupon instructor cho khóa học {course.Title}", 0, 0, 0, null, orderItems);

                var (isValidInstructorCoupon, instructorCouponError, instructorDiscount, _) = instructorCouponResult.Data;
                if (!isValidInstructorCoupon)
                    return (false, instructorCouponError, 0, 0, 0, null, orderItems);

                itemInstructorDiscount = instructorDiscount;
                totalInstructorDiscount += itemInstructorDiscount;

                logger.LogInformation("Applied instructor coupon {CouponCode} for course {CourseId}: discount {Discount}",
                    item.InstructorCouponCode, item.CourseId, itemInstructorDiscount);
            }

            // Calculate final price after instructor discount
            var finalPrice = itemSubTotal - itemInstructorDiscount;
            var pricing = CalculateOrderItemPricing(finalPrice);
            orderItems.Add(course.ToOrderItemSnapshot(unitPrice, 0, pricing)); // Store original price, discount handled at order level
            subTotal += pricing.LineTotal;
        }

        // ── Phase 2: Apply system coupon to the entire order ──
        decimal systemDiscount = 0;
        Guid? systemCouponId = null;
        if (!string.IsNullOrEmpty(systemCouponCode))
        {
            var systemCouponResult = await couponService.ValidateAndApplyCouponAsync(
                systemCouponCode, subTotal, items.Select(i => i.CourseId).ToList(), userId);

            if (!systemCouponResult.IsSuccess)
                return (false, systemCouponResult.Message ?? "Lỗi xác thực coupon hệ thống", 0, 0, 0, null, orderItems);

            var (isValidSystemCoupon, systemCouponError, systemDiscountAmount, systemCouponId_) = systemCouponResult.Data;
            if (!isValidSystemCoupon)
                return (false, systemCouponError, 0, 0, 0, null, orderItems);

            systemDiscount = systemDiscountAmount;
            systemCouponId = systemCouponId_;

            logger.LogInformation("Applied system coupon {CouponCode}: discount {Discount}", systemCouponCode, systemDiscount);
        }

        // ── Phase 3: Calculate final totals ──
        var totalDiscount = totalInstructorDiscount + systemDiscount;
        var totalAmount = Math.Max(0, subTotal - systemDiscount); // System discount applied to subtotal

        return (true, null, subTotal, totalDiscount, totalAmount, systemCouponId, orderItems);
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}
