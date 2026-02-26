using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Payments;

public class PaymentUrlResponse
{
    public bool IsPending { get; set; } = false;
    public PaymentStatus Status { get; set; }
    public Guid PaymentId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty; // OrderPayment | WalletTopUp | Subscription
    public string PaymentUrl { get; set; } = string.Empty;
    public DateTime ExpiredAt { get; set; }
}
