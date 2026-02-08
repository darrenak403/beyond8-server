using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Payments;

public static class PaymentMappings
{
    public static PaymentResponse ToResponse(this Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            PaymentNumber = payment.PaymentNumber,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Provider = payment.Provider,
            PaymentMethod = payment.PaymentMethod,
            ExternalTransactionId = payment.ExternalTransactionId,
            PaidAt = payment.PaidAt,
            ExpiredAt = payment.ExpiredAt,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }

    public static PaymentUrlResponse ToUrlResponse(this Payment payment, string paymentUrl)
    {
        return new PaymentUrlResponse
        {
            PaymentId = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            PaymentUrl = paymentUrl,
            ExpiredAt = payment.ExpiredAt ?? DateTime.UtcNow.AddMinutes(15)
        };
    }
}
