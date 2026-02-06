namespace Beyond8.Sale.Application.Dtos.Orders;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class OrderItemRequest
{
    public Guid CourseId { get; set; }
    public decimal Price { get; set; }
}