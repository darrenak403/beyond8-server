using Beyond8.Sale.Application.Dtos.OrderItems;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class CreateOrderRequest
{
    public Guid UserId { get; set; }
    public List<OrderItemRequest> Items { get; set; } = new();
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}