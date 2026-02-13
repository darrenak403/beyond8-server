using Beyond8.Sale.Application.Dtos.Payments;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Sale.Application.Dtos.Orders;
using Beyond8.Sale.Application.Mappings.Orders;

namespace Beyond8.Sale.Application.Mappings.Payments;

public static class PaymentMappings
{
    public static PaymentResponse ToResponse(this Payment payment)
    {
        // Check for pending payment info if this payment is for an order
        PendingPaymentResponse? pendingPaymentInfo = null;
        if (payment.Order != null &&
            payment.Order.Status == OrderStatus.Pending &&
            payment.Order.Payments != null)
        {
            var activePayment = payment.Order.Payments.FirstOrDefault(
                p => (p.Status == PaymentStatus.Pending || p.Status == PaymentStatus.Processing)
                && p.ExpiredAt > DateTime.UtcNow);

            if (activePayment != null && activePayment.Id == payment.Id)
            {
                pendingPaymentInfo = payment.Order.ToPendingPaymentResponse(activePayment);
            }
        }

        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            WalletId = payment.WalletId,
            PaymentNumber = payment.PaymentNumber,
            Purpose = payment.Purpose.ToString(),
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
            UpdatedAt = payment.UpdatedAt,
            PendingPaymentInfo = pendingPaymentInfo
        };
    }

    public static PaymentUrlResponse ToUrlResponse(this Payment payment, string paymentUrl)
    {
        return new PaymentUrlResponse
        {
            PaymentId = payment.Id,
            PaymentNumber = payment.PaymentNumber,
            Purpose = payment.Purpose.ToString(),
            PaymentUrl = paymentUrl,
            ExpiredAt = payment.ExpiredAt ?? DateTime.UtcNow.AddMinutes(15)
        };
    }
}
