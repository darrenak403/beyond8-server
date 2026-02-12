using Beyond8.Sale.Application.Dtos.OrderItems;

namespace Beyond8.Sale.Application.Dtos.Orders;

/// <summary>
/// Request for creating a new order (generic endpoint).
/// Recommended: Use BuyNowRequest for single course, use Cart Checkout for multiple courses.
/// </summary>
public class CreateOrderRequest
{
    /// <summary>
    /// List of courses to purchase.
    /// </summary>
    public List<OrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// System coupon code to apply to entire order (optional).
    /// Applied after instructor coupons to the subtotal.
    /// </summary>
    public string? CouponCode { get; set; }

    /// <summary>
    /// Additional notes for the order (optional).
    /// </summary>
    public string? Notes { get; set; }
}