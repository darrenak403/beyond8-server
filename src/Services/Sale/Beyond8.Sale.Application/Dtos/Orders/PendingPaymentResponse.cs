using Beyond8.Sale.Application.Dtos.Payments;

namespace Beyond8.Sale.Application.Dtos.Orders;

public class PendingPaymentResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public PaymentUrlResponse PaymentInfo { get; set; } = null!;
    public string Message { get; set; } = "Bạn có đơn hàng đang chờ thanh toán. Vui lòng hoàn tất thanh toán trước khi tạo đơn hàng mới.";
}