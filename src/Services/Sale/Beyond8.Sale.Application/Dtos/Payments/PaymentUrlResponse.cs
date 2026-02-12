namespace Beyond8.Sale.Application.Dtos.Payments;

public class PaymentUrlResponse
{
    public Guid PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // OrderPayment | WalletTopUp
    public string PaymentUrl { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
}
