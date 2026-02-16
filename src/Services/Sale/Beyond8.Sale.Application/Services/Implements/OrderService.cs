using Beyond8.Common;
using Beyond8.Common.Events;
using Beyond8.Common.Events.Cache;
using Beyond8.Common.Events.Sale;
using Beyond8.Common.Utilities;
using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Application.Mappings.Orders;
using Beyond8.Sale.Application.Services.Interfaces;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;
using static Beyond8.Sale.Application.Mappings.Orders.OrderMappings;

namespace Beyond8.Sale.Application.Services.Implements;

public class OrderService(
    ILogger<OrderService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    ICouponService couponService,
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

    /// <summary>
    /// Preview Buy Now pricing without creating an order.
    /// Validates coupons and calculates totals for single course purchase.
    /// </summary>
    public async Task<ApiResponse<PreviewBuyNowResponse>> PreviewBuyNowAsync(PreviewBuyNowRequest request, Guid userId)
    {
        // Check if user already purchased this course
        var alreadyPurchased = await unitOfWork.OrderRepository.AsQueryable()
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Paid)
            .SelectMany(o => o.OrderItems.Select(oi => oi.CourseId))
            .AnyAsync(courseId => courseId == request.CourseId);

        if (alreadyPurchased)
        {
            return ApiResponse<PreviewBuyNowResponse>.FailureResponse(
                "Bạn đã mua khóa học này rồi");
        }

        // Check if user is instructor of this course (prevent self-purchase)
        var courseResult = await catalogClient.GetCourseByIdAsync(request.CourseId);
        if (courseResult.IsSuccess && courseResult.Data != null)
        {
            if (courseResult.Data.InstructorId == userId)
            {
                return ApiResponse<PreviewBuyNowResponse>.FailureResponse(
                    "Bạn không thể mua khóa học của chính mình");
            }
        }
        else
        {
            return ApiResponse<PreviewBuyNowResponse>.FailureResponse(
                "Khóa học không tồn tại");
        }

        // Convert to preview order request for calculation
        var previewOrderRequest = new PreviewOrderRequest
        {
            Items = new List<PreviewOrderItemRequest>
            {
                new()
                {
                    CourseId = request.CourseId,
                    InstructorCouponCode = request.InstructorCouponCode
                }
            },
            CouponCode = request.CouponCode
        };

        // Use existing preview logic
        var previewResult = await PreviewOrderAsync(previewOrderRequest, userId);
        if (!previewResult.IsSuccess)
            return ApiResponse<PreviewBuyNowResponse>.FailureResponse(previewResult.Message ?? "Lỗi tính toán giá");

        var previewData = previewResult.Data!;
        var item = previewData.Items.First();

        var response = new PreviewBuyNowResponse
        {
            Item = item,
            SubTotal = previewData.SubTotal,
            SubTotalAfterInstructorDiscount = previewData.SubTotalAfterInstructorDiscount,
            InstructorDiscountAmount = previewData.InstructorDiscountAmount,
            SystemDiscountAmount = previewData.SystemDiscountAmount,
            TotalDiscountAmount = previewData.TotalDiscountAmount,
            TotalAmount = previewData.TotalAmount,
            IsFree = previewData.IsFree
        };

        return ApiResponse<PreviewBuyNowResponse>.SuccessResponse(response, "Tính toán giá thành công");
    }

    /// <summary>
    /// Preview order pricing without creating an order.
    /// Validates coupons and calculates totals for display purposes.
    /// </summary>
    public async Task<ApiResponse<PreviewOrderResponse>> PreviewOrderAsync(PreviewOrderRequest request, Guid userId)
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
            return ApiResponse<PreviewOrderResponse>.FailureResponse(
                "Bạn đã mua một hoặc nhiều khóa học trong đơn hàng này rồi");
        }

        // Check if user is instructor of any requested courses (prevent self-purchase)
        var instructorCourses = new List<Guid>();
        foreach (var item in request.Items)
        {
            var courseResult = await catalogClient.GetCourseByIdAsync(item.CourseId);
            if (courseResult.IsSuccess && courseResult.Data != null)
            {
                if (courseResult.Data.InstructorId == userId)
                {
                    instructorCourses.Add(item.CourseId);
                }
            }
        }

        if (instructorCourses.Any())
        {
            return ApiResponse<PreviewOrderResponse>.FailureResponse(
                "Bạn không thể mua khóa học của chính mình");
        }

        // Convert preview items to order items for calculation
        var orderItems = request.Items.Select(i => new OrderItemRequest
        {
            CourseId = i.CourseId,
            InstructorCouponCode = i.InstructorCouponCode
        }).ToList();

        // Validate and calculate totals
        var calculation = await CalculateOrderTotalsAsync(orderItems, request.CouponCode, userId);
        if (!calculation.IsValid)
            return ApiResponse<PreviewOrderResponse>.FailureResponse(calculation.ErrorMessage ?? "Lỗi tính toán giá");

        // Build preview response
        var previewItems = new List<PreviewOrderItemResponse>();
        for (int i = 0; i < request.Items.Count; i++)
        {
            var requestItem = request.Items[i];
            var calculatedItem = calculation.Items[i];

            var courseResult = await catalogClient.GetCourseByIdAsync(requestItem.CourseId);
            if (!courseResult.IsSuccess || courseResult.Data == null)
                continue;

            var course = courseResult.Data;

            previewItems.Add(new PreviewOrderItemResponse
            {
                CourseId = requestItem.CourseId,
                CourseTitle = course.Title,
                OriginalPrice = calculatedItem.OriginalPrice,
                InstructorDiscount = calculatedItem.InstructorDiscountAmount,
                FinalPrice = calculatedItem.LineTotal,
                InstructorCouponCode = requestItem.InstructorCouponCode,
                InstructorCouponApplied = !string.IsNullOrEmpty(requestItem.InstructorCouponCode) &&
                                         calculatedItem.InstructorDiscountAmount > 0
            });
        }

        var response = new PreviewOrderResponse
        {
            Items = previewItems,
            SubTotal = calculation.OriginalSubTotal,
            SubTotalAfterInstructorDiscount = calculation.SubTotalAfterInstructorDiscount,
            InstructorDiscountAmount = calculation.InstructorDiscountAmount,
            SystemDiscountAmount = calculation.SystemDiscountAmount,
            TotalDiscountAmount = calculation.TotalDiscountAmount,
            TotalAmount = calculation.TotalAmount,
            IsFree = calculation.TotalAmount == 0
        };

        return ApiResponse<PreviewOrderResponse>.SuccessResponse(response, "Tính toán giá thành công");
    }

    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request, Guid userId)
    {
        var strategy = unitOfWork.Context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await unitOfWork.Context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            // ── Re-check pending payment before creating new order (inside transaction) ──
            var pendingPaymentCheck = await CheckPendingPaymentAsync(userId);
            if (pendingPaymentCheck.HasPendingPayment)
            {
                var response = new OrderResponse
                {
                    PendingPaymentInfo = pendingPaymentCheck.PendingPaymentInfo
                };

                return ApiResponse<OrderResponse>.SuccessResponse(response, "Bạn có đơn hàng đang chờ thanh toán. Vui lòng hoàn tất thanh toán hoặc hủy đơn hàng đó trước khi tạo đơn hàng mới.");
            }

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

            // Check if user is instructor of any requested courses (prevent self-purchase)
            var instructorCourses = new List<Guid>();
            foreach (var item in request.Items)
            {
                var courseResult = await catalogClient.GetCourseByIdAsync(item.CourseId);
                if (courseResult.IsSuccess && courseResult.Data != null)
                {
                    if (courseResult.Data.InstructorId == userId)
                    {
                        instructorCourses.Add(item.CourseId);
                    }
                }
            }

            if (instructorCourses.Any())
            {
                logger.LogWarning("Instructor {UserId} attempted to purchase their own courses: {CourseIds}",
                    userId, string.Join(", ", instructorCourses));
                return ApiResponse<OrderResponse>.FailureResponse(
                    "Bạn không thể mua khóa học của chính mình");
            }

            // Validate and calculate totals
            var calculation = await CalculateOrderTotalsAsync(request.Items, request.CouponCode, userId);
            if (!calculation.IsValid)
                return ApiResponse<OrderResponse>.FailureResponse(calculation.ErrorMessage ?? "Lỗi tạo đơn hàng");

            // Create order with mapping
            var orderNumber = GenerateOrderNumber();
            var order = request.ToEntity(
                orderNumber,
                calculation.OriginalSubTotal,
                calculation.SubTotalAfterInstructorDiscount,
                calculation.TotalAmount,
                calculation.TotalDiscountAmount,
                calculation.InstructorDiscountAmount,
                calculation.SystemDiscountAmount,
                calculation.InstructorCouponId,
                calculation.SystemCouponId,
                userId);

            // Add order items with snapshot using mapping
            foreach (var item in calculation.Items)
            {
                order.OrderItems.Add(item.ToEntity(order.Id));
            }

            await unitOfWork.OrderRepository.AddAsync(order);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Order created: {OrderId} for user {UserId}", order.Id, userId);

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

            await transaction.CommitAsync();

            return ApiResponse<OrderResponse>.SuccessResponse(order.ToResponse(), "Đơn hàng đã được tạo thành công");
        });
    }

    /// <summary>
    /// Cancel order by order owner.
    /// Only allows cancelling orders in Pending status.
    /// Reverts coupon usage and updates payment status.
    /// </summary>
    public async Task<ApiResponse<OrderResponse>> CancelOrderAsync(Guid orderId, Guid userId)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            return ApiResponse<OrderResponse>.FailureResponse("Đơn hàng không tồn tại");

        // Check ownership
        if (order.UserId != userId)
            return ApiResponse<OrderResponse>.FailureResponse("Bạn không có quyền hủy đơn hàng này");

        // Only allow cancelling pending orders
        if (order.Status != OrderStatus.Pending)
            return ApiResponse<OrderResponse>.FailureResponse(
                $"Không thể hủy đơn hàng ở trạng thái {order.Status}. Chỉ có thể hủy đơn hàng đang chờ thanh toán.");

        // Update order status
        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        // ── Revert coupon usage ──
        if (order.SystemCouponId.HasValue)
        {
            try
            {
                var existingUsage = await unitOfWork.CouponUsageRepository.AsQueryable()
                    .Include(u => u.Coupon)
                    .FirstOrDefaultAsync(u => u.OrderId == orderId);

                if (existingUsage != null)
                {
                    // Remove the usage record
                    await unitOfWork.CouponUsageRepository.DeleteAsync(existingUsage.Id);

                    // Decrement coupon usage count
                    if (existingUsage.Coupon != null)
                    {
                        existingUsage.Coupon.UsedCount = Math.Max(0, existingUsage.Coupon.UsedCount - 1);
                        existingUsage.Coupon.UpdatedAt = DateTime.UtcNow;
                    }

                    await unitOfWork.SaveChangesAsync();

                    logger.LogInformation("Reverted coupon usage for cancelled order: {OrderId}", orderId);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to revert coupon usage for cancelled order: {OrderId}", orderId);
                // Don't throw - order cancellation should succeed even if coupon revert fails
            }
        }

        // ── Update payment status if exists ──
        var pendingPayment = order.Payments.FirstOrDefault(p => p.Status == PaymentStatus.Pending);
        if (pendingPayment != null)
        {
            pendingPayment.Status = PaymentStatus.Cancelled;
            pendingPayment.UpdatedAt = DateTime.UtcNow;

            logger.LogInformation("Cancelled pending payment {PaymentId} for order {OrderId}",
                pendingPayment.Id, orderId);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order cancelled: {OrderId} by user {UserId}", orderId, userId);

        return ApiResponse<OrderResponse>.SuccessResponse(
            order.ToResponse(), "Đơn hàng đã được hủy thành công");
    }

    public async Task<ApiResponse<OrderResponse>> GetOrderByIdAsync(Guid orderId)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .Include(o => o.Payments)
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

    public async Task<ApiResponse<bool>> IsCourseInPendingOrderAndPaymentAsync(Guid courseId, Guid userId)
    {
        try
        {
            var exists = await unitOfWork.OrderRepository.AsQueryable()
                .AsNoTracking()
                .AnyAsync(o => o.UserId == userId
                            && o.Status == OrderStatus.Pending
                            && o.OrderItems.Any(oi => oi.CourseId == courseId)
                            && o.Payments.Any(p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing));
            return ApiResponse<bool>.SuccessResponse(exists, "Kiểm tra đơn hàng thành công");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check pending order existence for user {UserId} course {CourseId}", userId, courseId);
            return ApiResponse<bool>.FailureResponse("Lỗi khi kiểm tra trạng thái đơn hàng");
        }
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
    private async Task<(bool HasPendingPayment, PendingPaymentResponse? PendingPaymentInfo)> CheckPendingPaymentAsync(Guid userId)
    {
        // Find the most recent pending order
        var pendingOrder = await unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.Payments)
            .Where(o => o.UserId == userId && o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();

        if (pendingOrder == null)
            return (false, null);

        // Check if there's any payment with status Pending or Processing (including expired ones)
        // This ensures consistency with background job that updates expired payments to Expired status
        var pendingPayment = pendingOrder.Payments.FirstOrDefault(
            p => p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing);

        // If there's a pending/processing payment, return its info (even if expired)
        if (pendingPayment != null)
        {
            var pendingPaymentInfo = pendingOrder.ToPendingPaymentResponse(pendingPayment);
            return (true, pendingPaymentInfo);
        }

        // If no pending/processing payments (all completed, failed, or expired), allow new order creation
        return (false, null);
    }

    private async Task<(bool IsValid, string? ErrorMessage, decimal OriginalSubTotal, decimal SubTotalAfterInstructorDiscount, decimal InstructorDiscountAmount, decimal SystemDiscountAmount, decimal TotalDiscountAmount, decimal TotalAmount, Guid? InstructorCouponId, Guid? SystemCouponId, List<OrderItemSnapshot> Items)> CalculateOrderTotalsAsync(List<OrderItemRequest> items, string? systemCouponCode, Guid userId)
    {
        var orderItems = new List<OrderItemSnapshot>();
        decimal originalSubTotal = 0;
        decimal subTotalAfterInstructorDiscount = 0;
        decimal totalInstructorDiscount = 0;
        Guid? instructorCouponId = null; // Track the first instructor coupon used

        // ── Phase 1: Calculate base prices and apply instructor coupons per item ──
        foreach (var item in items)
        {
            var courseResult = await catalogClient.GetCourseByIdAsync(item.CourseId);
            if (!courseResult.IsSuccess)
                return (false, $"Khóa học không tồn tại", 0, 0, 0, 0, 0, 0, null, null, orderItems);

            if (courseResult.Data == null)
                return (false, "Dữ liệu khóa học không hợp lệ", 0, 0, 0, 0, 0, 0, null, null, orderItems);

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
                    return (false, instructorCouponResult.Message ?? $"Lỗi xác thực coupon instructor cho khóa học {course.Title}", 0, 0, 0, 0, 0, 0, null, null, orderItems);

                var (isValidInstructorCoupon, instructorCouponError, instructorDiscount, instructorCouponId_) = instructorCouponResult.Data;
                if (!isValidInstructorCoupon)
                    return (false, instructorCouponError, 0, 0, 0, 0, 0, 0, null, null, orderItems);

                itemInstructorDiscount = instructorDiscount;
                totalInstructorDiscount += itemInstructorDiscount;

                // Track the first instructor coupon ID used
                if (instructorCouponId == null && instructorCouponId_.HasValue)
                    instructorCouponId = instructorCouponId_.Value;

                logger.LogInformation("Applied instructor coupon {CouponCode} for course {CourseId}: discount {Discount}",
                    item.InstructorCouponCode, item.CourseId, itemInstructorDiscount);
            }

            // Calculate final price after instructor discount
            var finalPrice = itemSubTotal - itemInstructorDiscount;

            // Calculate discount percent from instructor coupon
            var discountPercent = itemInstructorDiscount > 0 ? (itemInstructorDiscount / unitPrice) * 100 : 0;

            var pricing = CalculateOrderItemPricing(finalPrice, itemInstructorDiscount);
            orderItems.Add(course.ToOrderItemSnapshot(unitPrice, discountPercent, pricing));
            originalSubTotal += unitPrice; // Sum of original prices
            subTotalAfterInstructorDiscount += pricing.LineTotal; // Sum after instructor discounts
        }

        // ── Phase 2: Apply system coupon to the entire order ──
        decimal systemDiscount = 0;
        Guid? systemCouponId = null;
        if (!string.IsNullOrEmpty(systemCouponCode))
        {
            var systemCouponResult = await couponService.ValidateAndApplyCouponAsync(
                systemCouponCode, subTotalAfterInstructorDiscount, items.Select(i => i.CourseId).ToList(), userId);

            if (!systemCouponResult.IsSuccess)
                return (false, systemCouponResult.Message ?? "Lỗi xác thực coupon hệ thống", 0, 0, 0, 0, 0, 0, null, null, orderItems);

            var (isValidSystemCoupon, systemCouponError, systemDiscountAmount, systemCouponId_) = systemCouponResult.Data;
            if (!isValidSystemCoupon)
                return (false, systemCouponError, 0, 0, 0, 0, 0, 0, null, null, orderItems);

            systemDiscount = systemDiscountAmount;
            systemCouponId = systemCouponId_;

            logger.LogInformation("Applied system coupon {CouponCode}: discount {Discount}", systemCouponCode, systemDiscount);
        }

        // ── Phase 3: Calculate final totals ──
        var totalDiscount = totalInstructorDiscount + systemDiscount;
        var totalAmount = Math.Max(0, subTotalAfterInstructorDiscount - systemDiscount); // System discount applied to subtotal after instructor discounts

        // ── Phase 4: Recalculate platform fee and instructor earnings after system discount ──
        // System discount affects the final amount, so we need to redistribute platform fee and instructor earnings proportionally
        if (systemDiscount > 0 && subTotalAfterInstructorDiscount > 0)
        {
            var discountRatio = totalAmount / subTotalAfterInstructorDiscount; // How much of original amount remains after system discount

            foreach (var item in orderItems)
            {
                // Recalculate platform fee and instructor earnings based on final amount
                // LineTotal remains as UnitPrice * Quantity (after instructor discount only)
                var adjustedPlatformFee = item.LineTotal * discountRatio * 0.30m; // Per BR-19: 30% platform fee
                var adjustedInstructorEarnings = item.LineTotal * discountRatio - adjustedPlatformFee; // 70% to instructor

                // Update the snapshot with adjusted values (keep LineTotal unchanged)
                item.PlatformFeeAmount = adjustedPlatformFee;
                item.InstructorEarnings = adjustedInstructorEarnings;
            }

            logger.LogInformation("Recalculated platform fee and instructor earnings after system discount: ratio {Ratio}, totalAmount {TotalAmount}",
                discountRatio, totalAmount);
        }

        return (true, null, originalSubTotal, subTotalAfterInstructorDiscount, totalInstructorDiscount, systemDiscount, totalDiscount, totalAmount, instructorCouponId, systemCouponId, orderItems);
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }

    private string GeneratePaymentNumber()
    {
        return $"PAY-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}
