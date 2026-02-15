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
    public string? PaymentUrl { get; set; } // URL for payment redirect
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiredAt { get; set; }
    public string? FailureReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Essential order information when purchasing courses
    public OrderSummary? OrderSummary { get; set; }

    // Additional metadata
    public string? Metadata { get; set; }

    // Pending payment info (for blocked orders)
    public PendingPaymentResponse? PendingPaymentInfo { get; set; }
}

public class OrderSummary
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";
    public List<OrderItemSummary> Items { get; set; } = new();
}

public class OrderItemSummary
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseSlug { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal FinalPrice { get; set; }
    public string InstructorName { get; set; } = string.Empty;
}