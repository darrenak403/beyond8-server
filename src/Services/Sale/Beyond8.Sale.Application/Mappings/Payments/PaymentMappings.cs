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
            (payment.Status == PaymentStatus.Pending || payment.Status == PaymentStatus.Processing) &&
            payment.ExpiredAt > DateTime.UtcNow)
        {
            pendingPaymentInfo = payment.Order.ToPendingPaymentResponse(payment);
        }

        // For subscription payments (no Order), expose pending info similarly so clients can show pending subscription purchase
        if (pendingPaymentInfo == null && payment.Purpose == PaymentPurpose.Subscription
            && (payment.Status == PaymentStatus.Pending || payment.Status == PaymentStatus.Processing)
            && payment.ExpiredAt > DateTime.UtcNow)
        {
            pendingPaymentInfo = new PendingPaymentResponse
            {
                OrderId = payment.Id, // use payment.Id as placeholder
                OrderNumber = payment.PaymentNumber,
                PaymentInfo = payment.ToUrlResponse(payment.PaymentUrl ?? string.Empty)
            };
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
            PaymentUrl = payment.PaymentUrl,
            PaidAt = payment.PaidAt,
            ExpiredAt = payment.ExpiredAt,
            FailureReason = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt,
            OrderSummary = payment.Order?.ToOrderSummary(),
            Metadata = payment.Metadata,
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
            ExpiredAt = payment.ExpiredAt ?? DateTime.UtcNow.AddMinutes(15),
            Status = payment.Status
        };
    }

    public static PendingPaymentResponse ToPendingPaymentResponse(this Payment payment)
    {
        // If the payment belongs to an Order, reuse the Order -> PendingPaymentResponse mapping
        if (payment.Order != null)
            return payment.Order.ToPendingPaymentResponse(payment);

        // Otherwise build a subscription/non-order pending response
        return new PendingPaymentResponse
        {
            OrderId = payment.Id,
            OrderNumber = payment.PaymentNumber,
            PaymentInfo = payment.ToUrlResponse(payment.PaymentUrl ?? string.Empty)
        };
    }
}
