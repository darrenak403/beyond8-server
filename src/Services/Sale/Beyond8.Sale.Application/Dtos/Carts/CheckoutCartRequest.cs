namespace Beyond8.Sale.Application.Dtos.Carts;

public class CheckoutCartRequest
{
    public List<Guid> SelectedCourseIds { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}
