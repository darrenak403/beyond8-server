using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class TransactionLedger : BaseEntity
{
    // Wallet Reference
    public Guid WalletId { get; set; }

    [ForeignKey(nameof(WalletId))]
    public virtual InstructorWallet InstructorWallet { get; set; } = null!;

    public Guid? ReferenceId { get; set; }

    [MaxLength(50)]
    public string? ReferenceType { get; set; }

    // Transaction Details
    public TransactionType Type { get; set; }

    public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Balance Tracking (Audit Trail)
    [Column(TypeName = "decimal(18, 2)")]
    public decimal BalanceBefore { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BalanceAfter { get; set; }

    // Description & Metadata
    [MaxLength(500)]
    public string? Description { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; } // Additional data (order details, payout info, etc.)

    // External Transaction Reference
    [MaxLength(200)]
    public string? ExternalTransactionId { get; set; } // Bank transaction ID for payouts
}
