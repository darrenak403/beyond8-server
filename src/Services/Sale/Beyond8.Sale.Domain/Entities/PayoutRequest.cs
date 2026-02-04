using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;
using System.ComponentModel.DataAnnotations;

namespace Beyond8.Sale.Domain.Entities;

public class PayoutRequest : BaseEntity
{
    public Guid InstructorId { get; set; }

    public Guid InstructorWalletId { get; set; }

    [ForeignKey(nameof(InstructorWalletId))]
    public virtual InstructorWallet InstructorWallet { get; set; } = null!;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public PayoutStatus Status { get; set; } = PayoutStatus.Requested;

    public DateTime RequestedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    [MaxLength(500)]
    public string? BankAccountInfo { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}
