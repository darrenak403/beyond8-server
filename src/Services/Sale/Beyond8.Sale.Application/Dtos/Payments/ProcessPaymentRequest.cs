namespace Beyond8.Sale.Application.Dtos.Payments;

public class ProcessPaymentRequest
{
    public Guid OrderId { get; set; }
    public string PaymentMethod { get; set; } = "VNPay";
    public string? BankCode { get; set; }
}
