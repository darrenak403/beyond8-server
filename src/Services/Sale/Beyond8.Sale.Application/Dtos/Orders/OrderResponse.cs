using Beyond8.Sale.Application.Dtos.OrderItems;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }

    // Pricing
    public decimal SubTotal { get; set; } // Original subtotal (sum of all original prices)
    public decimal SubTotalAfterInstructorDiscount { get; set; } // Subtotal after instructor discounts applied
    public decimal InstructorDiscountAmount { get; set; }
    public decimal SystemDiscountAmount { get; set; }
    public decimal DiscountAmount { get; set; } // Total discount (Instructor + System)
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";

    // Revenue Split (Per BR-19: 30% platform, 70% instructor)
    public decimal PlatformFeeAmount { get; set; }
    public decimal InstructorEarnings { get; set; }

    // Coupons
    public Guid? InstructorCouponId { get; set; }
    public Guid? SystemCouponId { get; set; }

    // Payment
    public DateTime? PaidAt { get; set; }

    // Security
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }

    // Items
    public List<OrderItemResponse> OrderItems { get; set; } = new();

    // Pending Payment (when order creation is blocked due to existing pending payment)
    public PendingPaymentResponse? PendingPaymentInfo { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}