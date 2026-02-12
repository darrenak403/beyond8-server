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
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";

    // Coupon
    public Guid? CouponId { get; set; }

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