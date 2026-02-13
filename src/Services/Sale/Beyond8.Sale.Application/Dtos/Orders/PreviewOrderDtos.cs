namespace Beyond8.Sale.Application.Dtos.Orders;

/// <summary>
/// Request for order price preview - calculates totals without creating order.
/// </summary>
public class PreviewOrderRequest
{
    /// <summary>
    /// List of items to preview pricing for.
    /// </summary>
    public List<PreviewOrderItemRequest> Items { get; set; } = new();

    /// <summary>
    /// System coupon code to apply to entire order (optional).
    /// </summary>
    public string? CouponCode { get; set; }
}

/// <summary>
/// Individual item in preview request.
/// </summary>
public class PreviewOrderItemRequest
{
    /// <summary>
    /// Course ID to preview.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Instructor coupon code for this specific course (optional).
    /// </summary>
    public string? InstructorCouponCode { get; set; }
}

/// <summary>
/// Response containing preview pricing information.
/// </summary>
public class PreviewOrderResponse
{
    /// <summary>
    /// List of items with calculated pricing.
    /// </summary>
    public List<PreviewOrderItemResponse> Items { get; set; } = new();

    /// <summary>
    /// Subtotal before any discounts.
    /// </summary>
    public decimal SubTotal { get; set; }

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
    /// Whether the order would be free (TotalAmount = 0).
    /// </summary>
    public bool IsFree { get; set; }
}

/// <summary>
/// Individual item pricing in preview response.
/// </summary>
public class PreviewOrderItemResponse
{
    /// <summary>
    /// Course ID.
    /// </summary>
    public Guid CourseId { get; set; }

    /// <summary>
    /// Course title.
    /// </summary>
    public string CourseTitle { get; set; } = string.Empty;

    /// <summary>
    /// Original course price.
    /// </summary>
    public decimal OriginalPrice { get; set; }

    /// <summary>
    /// Instructor discount applied to this item.
    /// </summary>
    public decimal InstructorDiscount { get; set; }

    /// <summary>
    /// Final price after instructor discount.
    /// </summary>
    public decimal FinalPrice { get; set; }

    /// <summary>
    /// Platform fee for this item.
    /// </summary>
    public decimal PlatformFee { get; set; }

    /// <summary>
    /// Instructor earnings for this item.
    /// </summary>
    public decimal InstructorEarnings { get; set; }

    /// <summary>
    /// Instructor coupon code applied (if any).
    /// </summary>
    public string? InstructorCouponCode { get; set; }

    /// <summary>
    /// Whether instructor coupon was applied successfully.
    /// </summary>
    public bool InstructorCouponApplied { get; set; }
}