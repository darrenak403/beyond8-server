namespace Beyond8.Sale.Application.Dtos.Orders;

/// <summary>
/// Request for Buy Now price preview - calculates pricing for single course without creating order.
/// </summary>
public class PreviewBuyNowRequest
{
    /// <summary>
    /// Course ID to preview pricing for.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Instructor coupon code for this course (optional).
    /// </summary>
    public string? InstructorCouponCode { get; set; }

    /// <summary>
    /// System coupon code to apply to order (optional).
    /// </summary>
    public string? CouponCode { get; set; }
}

/// <summary>
/// Response for Buy Now preview - contains pricing information for single course.
/// </summary>
public class PreviewBuyNowResponse
{
    /// <summary>
    /// Course information with pricing.
    /// </summary>
    public PreviewOrderItemResponse Item { get; set; } = new();

    /// <summary>
    /// Subtotal of original course price.
    /// </summary>
    public decimal SubTotal { get; set; }

    /// <summary>
    /// Subtotal after instructor discount applied.
    /// </summary>
    public decimal SubTotalAfterInstructorDiscount { get; set; }

    /// <summary>
    /// Total instructor discount amount.
    /// </summary>
    public decimal InstructorDiscountAmount { get; set; }

    /// <summary>
    /// Total system discount amount.
    /// </summary>
    public decimal SystemDiscountAmount { get; set; }

    /// <summary>
    /// Total discount amount (instructor + system).
    /// </summary>
    public decimal TotalDiscountAmount { get; set; }

    /// <summary>
    /// Final total amount after all discounts.
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// Whether the purchase would be free (TotalAmount = 0).
    /// </summary>
    public bool IsFree { get; set; }
}