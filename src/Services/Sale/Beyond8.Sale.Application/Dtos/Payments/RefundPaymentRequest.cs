namespace Beyond8.Sale.Application.Dtos.Payments;

public class RefundPaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
}