namespace Beyond8.Sale.Application.Dtos.Orders;

/// <summary>
/// Request for Buy Now - Direct single course purchase.
/// Use Cart Checkout for multiple courses.
/// </summary>
public class BuyNowRequest
{
    /// <summary>
    /// Course ID to purchase.
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

    /// <summary>
    /// Additional notes for the order (optional).
    /// </summary>
    public string? Notes { get; set; }
}
