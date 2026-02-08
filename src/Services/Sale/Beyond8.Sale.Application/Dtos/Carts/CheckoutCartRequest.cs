namespace Beyond8.Sale.Application.Dtos.Carts;

public class CheckoutCartRequest
{
    public List<CheckoutItemRequest> SelectedItems { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class CheckoutItemRequest
{
    public Guid CourseId { get; set; }
    public string? InstructorCouponCode { get; set; }
}
