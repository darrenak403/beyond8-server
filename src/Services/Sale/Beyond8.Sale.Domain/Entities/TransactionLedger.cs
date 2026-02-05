using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class TransactionLedger : BaseEntity
{
    public Guid InstructorWalletId { get; set; }

    [ForeignKey(nameof(InstructorWalletId))]
    public virtual InstructorWallet InstructorWallet { get; set; } = null!;

    public Guid? OrderId { get; set; }

    public Guid? OrderItemId { get; set; }

    public TransactionType Type { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Pending;

    [MaxLength(500)]
    public string? Description { get; set; }
}
