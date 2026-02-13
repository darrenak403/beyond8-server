using Beyond8.Sale.Application.Clients.Catalog;
using Beyond8.Sale.Application.Dtos.Courses;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Application.Dtos.Payments;

namespace Beyond8.Sale.Application.Mappings.Orders;

public static class OrderMappings
{
    public static Order ToEntity(this CreateOrderRequest request, string orderNumber, decimal subTotal, decimal totalAmount, decimal totalDiscountAmount, decimal instructorDiscountAmount, decimal systemDiscountAmount, Guid? instructorCouponId, Guid? systemCouponId, Guid userId)
    {
        return new Order
        {
            UserId = userId,
            OrderNumber = orderNumber,
            Status = totalAmount == 0 ? OrderStatus.Paid : OrderStatus.Pending,
            SubTotal = subTotal,
            InstructorDiscountAmount = instructorDiscountAmount,
            SystemDiscountAmount = systemDiscountAmount,
            DiscountAmount = totalDiscountAmount,
            TotalAmount = totalAmount,
            InstructorCouponId = instructorCouponId,
            SystemCouponId = systemCouponId,
            Currency = "VND",
            PaidAt = totalAmount == 0 ? DateTime.UtcNow : null,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static OrderResponse ToResponse(this Order order)
    {
        // Calculate final revenue split after all discounts (Per BR-19: 30% platform, 70% instructor)
        var finalAmount = order.TotalAmount;
        var platformFeeAmount = finalAmount * 0.30m;
        var instructorEarnings = finalAmount - platformFeeAmount;

        // Check for active pending payment
        PendingPaymentResponse? pendingPaymentInfo = null;
        if (order.Status == OrderStatus.Pending && order.Payments != null)
        {
            var activePayment = order.Payments.FirstOrDefault(
                p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                && p.ExpiredAt > DateTime.UtcNow);

            if (activePayment != null)
            {
                pendingPaymentInfo = order.ToPendingPaymentResponse(activePayment);
            }
        }

        return new OrderResponse
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            SubTotal = order.SubTotal,
            InstructorDiscountAmount = order.InstructorDiscountAmount,
            SystemDiscountAmount = order.SystemDiscountAmount,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount ?? 0,
            TotalAmount = order.TotalAmount,
            PlatformFeeAmount = platformFeeAmount,
            InstructorEarnings = instructorEarnings,
            InstructorCouponId = order.InstructorCouponId,
            SystemCouponId = order.SystemCouponId,
            Currency = order.Currency,
            PaidAt = order.PaidAt,
            IpAddress = order.IpAddress,
            UserAgent = order.UserAgent,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            OrderItems = order.OrderItems.Select(oi => oi.ToResponse()).ToList(),
            PendingPaymentInfo = pendingPaymentInfo
        };
    }

    public static OrderItemResponse ToResponse(this OrderItem item)
    {
        return new OrderItemResponse
        {
            Id = item.Id,
            CourseId = item.CourseId,
            CourseTitle = item.CourseTitle,
            CourseThumbnail = item.CourseThumbnail,
            InstructorId = item.InstructorId,
            InstructorName = item.InstructorName,
            OriginalPrice = item.OriginalPrice,
            UnitPrice = item.UnitPrice,
            DiscountPercent = item.DiscountPercent,
            InstructorDiscountAmount = item.InstructorDiscountAmount,
            Quantity = item.Quantity,
            LineTotal = item.LineTotal
        };
    }

    /// <summary>
    /// Maps OrderItemSnapshot (temporary data structure) to persistent OrderItem entity
    /// </summary>
    public static OrderItem ToEntity(this OrderItemSnapshot snapshot, Guid orderId)
    {
        return new OrderItem
        {
            OrderId = orderId,
            CourseId = snapshot.CourseId,
            CourseTitle = snapshot.CourseTitle,
            CourseThumbnail = snapshot.CourseThumbnail,
            InstructorId = snapshot.InstructorId,
            InstructorName = snapshot.InstructorName,
            OriginalPrice = snapshot.OriginalPrice,
            UnitPrice = snapshot.UnitPrice,
            DiscountPercent = snapshot.DiscountPercent,
            InstructorDiscountAmount = snapshot.InstructorDiscountAmount,
            Quantity = 1, // Always 1 for courses
            LineTotal = snapshot.LineTotal,
            PlatformFeePercent = 0.30m, // Per BR-19: 30%
            PlatformFeeAmount = snapshot.PlatformFeeAmount,
            InstructorEarnings = snapshot.InstructorEarnings
        };
    }

    /// <summary>
    /// Calculates pricing (lineTotal, platformFee, instructorEarnings) for a course item
    /// Per BR-19: Platform gets 30%, Instructor gets 70%
    /// </summary>
    public record OrderItemPricing(decimal LineTotal, decimal PlatformFeeAmount, decimal InstructorEarnings, decimal InstructorDiscountAmount);

    public static OrderItemPricing CalculateOrderItemPricing(decimal unitPrice, decimal instructorDiscountAmount = 0)
    {
        var lineTotal = unitPrice * 1; // Quantity = 1 for courses
        var platformFeeAmount = lineTotal * 0.30m; // Per BR-19: 30% platform fee
        var instructorEarnings = lineTotal - platformFeeAmount; // 70% to instructor
        return new OrderItemPricing(lineTotal, platformFeeAmount, instructorEarnings, instructorDiscountAmount);
    }

    /// <summary>
    /// Maps CourseDto + calculated pricing to OrderItemSnapshot for order processing
    /// </summary>
    public static OrderItemSnapshot ToOrderItemSnapshot(
        this CourseDto course,
        decimal unitPrice,
        decimal discountPercent,
        OrderItemPricing pricing)
    {
        return new OrderItemSnapshot
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            CourseThumbnail = course.ThumbnailUrl,
            InstructorId = course.InstructorId,
            InstructorName = course.InstructorName,
            OriginalPrice = course.OriginalPrice,
            UnitPrice = unitPrice,
            DiscountPercent = discountPercent,
            InstructorDiscountAmount = pricing.InstructorDiscountAmount,
            LineTotal = pricing.LineTotal,
            PlatformFeeAmount = pricing.PlatformFeeAmount,
            InstructorEarnings = pricing.InstructorEarnings
        };
    }
    public static PendingPaymentResponse ToPendingPaymentResponse(this Order order, Payment activePayment)
    {
        return new PendingPaymentResponse
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            PaymentInfo = new PaymentUrlResponse
            {
                PaymentId = activePayment.Id,
                PaymentNumber = activePayment.PaymentNumber,
                Purpose = activePayment.Purpose.ToString(),
                PaymentUrl = activePayment.PaymentUrl ?? string.Empty,
                ExpiredAt = activePayment.ExpiredAt ?? DateTime.UtcNow.AddMinutes(15)
            }
        };
    }

    /// <summary>
    /// Internal data structure for snapshot during order calculation
    /// </summary>
    public class OrderItemSnapshot
    {
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseThumbnail { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal InstructorDiscountAmount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal PlatformFeeAmount { get; set; }
        public decimal InstructorEarnings { get; set; }
    }
}