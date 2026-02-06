using Beyond8.Sale.Application.Dtos.OrderItems;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class OrderResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    // Pricing
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";

    // Coupon
    public Guid? CouponId { get; set; }
    public string? CouponCode { get; set; }

    // Payment
    public DateTime? PaidAt { get; set; }

    public string? Notes { get; set; }

    // Items
    public List<OrderItemResponse> Items { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}