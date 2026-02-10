using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class Payment : BaseEntity
{
    // Order Reference (nullable for WalletTopUp payments)
    public Guid? OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order? Order { get; set; }

    /// <summary>
    /// Distinguishes order payment from wallet top-up
    /// </summary>
    public PaymentPurpose Purpose { get; set; } = PaymentPurpose.OrderPayment;

    /// <summary>
    /// Target wallet for WalletTopUp payments (null for OrderPayment)
    /// </summary>
    public Guid? WalletId { get; set; }

    [ForeignKey(nameof(WalletId))]
    public virtual InstructorWallet? InstructorWallet { get; set; }

    // Payment Identification
    [Required, MaxLength(50)]
    public string PaymentNumber { get; set; } = string.Empty;

    // Payment Status
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    // Amount Information
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Payment Provider Information
    [Required, MaxLength(50)]
    public string Provider { get; set; } = string.Empty; // VNPay, PayOS, ZaloPay, etc.

    [MaxLength(50)]
    public string? PaymentMethod { get; set; } // ATM, Visa, QR, etc.

    [MaxLength(200)]
    public string? ExternalTransactionId { get; set; } // Transaction ID from payment provider

    // Timestamps
    public DateTime? PaidAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    // Failure Information
    [MaxLength(500)]
    public string? FailureReason { get; set; }

    // TODO: Refund fields - RefundedAmount, RefundTransactionId, RefundedAt

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
}
