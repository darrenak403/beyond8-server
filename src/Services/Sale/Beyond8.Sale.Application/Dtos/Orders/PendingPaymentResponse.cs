using Beyond8.Sale.Application.Dtos.Payments;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class PendingPaymentResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public PaymentUrlResponse PaymentInfo { get; set; } = null!;
}