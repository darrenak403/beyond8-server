using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    [MaxLength(50)]
    public string Provider { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? ExternalTransactionId { get; set; }

    public DateTime? PaidAt { get; set; }
}
