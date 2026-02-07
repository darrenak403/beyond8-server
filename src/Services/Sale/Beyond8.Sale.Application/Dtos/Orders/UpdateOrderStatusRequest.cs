using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class UpdateOrderStatusRequest
{
    public OrderStatus Status { get; set; }
}