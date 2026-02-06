using Beyond8.Common;
using Beyond8.Common.Events;
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

namespace Beyond8.Sale.Application.Services.Implements;

public class OrderService(
    ILogger<OrderService> logger,
    IUnitOfWork unitOfWork,
    ICatalogClient catalogClient,
    ICouponService couponService,
    IPublishEndpoint publishEndpoint) : IOrderService
{
    public async Task<ApiResponse<OrderResponse>> CreateOrderAsync(CreateOrderRequest request)
    {
        // Validate and calculate totals
        var calculation = await CalculateOrderTotalsAsync(request.Items, request.CouponCode);
        if (!calculation.IsValid)
            return ApiResponse<OrderResponse>.FailureResponse(calculation.ErrorMessage ?? "Lỗi tạo đơn hàng");

        // Create order with mapping
        var orderNumber = GenerateOrderNumber();
        var order = request.ToEntity(orderNumber, calculation.TotalAmount, calculation.DiscountAmount, calculation.CouponId);

        // Add order items with snapshot using mapping
        foreach (var item in calculation.Items)
        {
            order.OrderItems.Add(item.ToEntity(order.Id));
        }

        await unitOfWork.OrderRepository.AddAsync(order);
        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order created: {OrderId} for user {UserId}", order.Id, request.UserId);

        // Publish event for free courses (BR-04)
        if (order.Status == OrderStatus.Paid)
        {
            await publishEndpoint.Publish(new OrderCompletedEvent(
                order.Id,
                request.UserId,
                request.Items.Select(i => i.CourseId).ToList()));
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

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid orderId)
    {
        var order = await unitOfWork.OrderRepository.AsQueryable()
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null)
            return ApiResponse<bool>.FailureResponse("Đơn hàng không tồn tại");

        if (order.Status != OrderStatus.Pending)
            return ApiResponse<bool>.FailureResponse("Chỉ hủy đơn hàng đang chờ xử lý");

        order.Status = OrderStatus.Cancelled;
        order.UpdatedAt = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation("Order cancelled: {OrderId}", orderId);

        return ApiResponse<bool>.SuccessResponse(true, "Đơn hàng đã được hủy");
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

    public async Task<ApiResponse<OrderStatisticsResponse>> GetOrderStatisticsAsync(Guid? instructorId = null)
    {
        // Use database-level aggregation instead of loading all orders into memory
        var query = unitOfWork.OrderRepository.AsQueryable()
            .Include(o => o.OrderItems)
            .AsNoTracking();

        if (instructorId.HasValue)
            query = query.Where(o => o.OrderItems.Any(oi => oi.InstructorId == instructorId));

        var totalOrders = await query.CountAsync();
        var completedOrders = await query.CountAsync(o => o.Status == OrderStatus.Paid);
        var pendingOrders = await query.CountAsync(o => o.Status == OrderStatus.Pending);
        var cancelledOrders = await query.CountAsync(o => o.Status == OrderStatus.Cancelled);

        var paidOrders = query.Where(o => o.Status == OrderStatus.Paid);

        var totalRevenue = await paidOrders.SumAsync(o => o.TotalAmount);

        decimal totalPlatformFees = 0;
        decimal totalInstructorEarnings = 0;

        if (instructorId.HasValue)
        {
            totalPlatformFees = await paidOrders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.InstructorId == instructorId)
                .SumAsync(oi => oi.PlatformFeeAmount);

            totalInstructorEarnings = await paidOrders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi.InstructorId == instructorId)
                .SumAsync(oi => oi.InstructorEarnings);
        }
        else
        {
            totalPlatformFees = await paidOrders
                .SelectMany(o => o.OrderItems)
                .SumAsync(oi => oi.PlatformFeeAmount);

            totalInstructorEarnings = await paidOrders
                .SelectMany(o => o.OrderItems)
                .SumAsync(oi => oi.InstructorEarnings);
        }

        var stats = new OrderStatisticsResponse
        {
            TotalOrders = totalOrders,
            CompletedOrders = completedOrders,
            PendingOrders = pendingOrders,
            CancelledOrders = cancelledOrders,
            TotalRevenue = totalRevenue,
            TotalPlatformFees = totalPlatformFees,
            TotalInstructorEarnings = totalInstructorEarnings,
            AverageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0,
            InstructorId = instructorId
        };

        return ApiResponse<OrderStatisticsResponse>.SuccessResponse(stats, "Lấy thống kê đơn hàng thành công");
    }

    // Helper methods
    private async Task<(bool IsValid, string? ErrorMessage, decimal SubTotal, decimal DiscountAmount, decimal TotalAmount, Guid? CouponId, List<OrderMappings.OrderItemSnapshot> Items)> CalculateOrderTotalsAsync(List<OrderItemRequest> items, string? couponCode)
    {
        var orderItems = new List<OrderMappings.OrderItemSnapshot>();
        decimal subTotal = 0;

        foreach (var item in items)
        {
            var courseResult = await catalogClient.GetCourseByIdAsync(item.CourseId);
            if (!courseResult.IsSuccess)
                return (false, $"Khóa học không tồn tại", 0, 0, 0, null, orderItems);

            if (courseResult.Data == null)
                return (false, "Dữ liệu khóa học không hợp lệ", 0, 0, 0, null, orderItems);

            var course = courseResult.Data;
            var unitPrice = item.Price;
            if (unitPrice != course.OriginalPrice)
                return (false, $"Giá khóa học không hợp lệ (giá yêu cầu: {unitPrice}, giá thực tế: {course.OriginalPrice})", 0, 0, 0, null, orderItems);

            // Per BR-19: No instructor discount for now
            var discountPercent = 0m;
            var pricing = OrderMappings.CalculateOrderItemPricing(unitPrice);
            orderItems.Add(course.ToOrderItemSnapshot(unitPrice, discountPercent, pricing));
            subTotal += pricing.LineTotal;
        }

        decimal discountAmount = 0;
        Guid? couponId = null;
        if (!string.IsNullOrEmpty(couponCode))
        {
            var couponResult = await couponService.ValidateAndApplyCouponAsync(couponCode, subTotal, items.Select(i => i.CourseId).ToList());
            if (!couponResult.IsSuccess)
                return (false, couponResult.Message ?? "Lỗi xác thực coupon", 0, 0, 0, null, orderItems);

            var (isValidCoupon, couponErrorMsg, couponDiscount, couponId_) = couponResult.Data;
            if (!isValidCoupon)
                return (false, couponErrorMsg, 0, 0, 0, null, orderItems);

            discountAmount = couponDiscount;
            couponId = couponId_;
        }
        var totalAmount = Math.Max(0, subTotal - discountAmount);
        return (true, null, subTotal, discountAmount, totalAmount, couponId, orderItems);
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper()}";
    }
}
