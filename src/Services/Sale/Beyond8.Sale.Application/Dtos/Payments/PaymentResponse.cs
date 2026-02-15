using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Application.Dtos.Orders;

namespace Beyond8.Sale.Application.Dtos.Payments;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid? OrderId { get; set; }
    public Guid? WalletId { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public string Provider { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? ExternalTransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public PendingPaymentResponse? PendingPaymentInfo { get; set; }
}