using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Transaction record for platform wallet operations.
/// Tracks revenue credits and coupon cost debits.
/// </summary>
public class PlatformWalletTransaction : BaseEntity
{
    // Wallet Reference
    public Guid PlatformWalletId { get; set; }

    [ForeignKey(nameof(PlatformWalletId))]
    public virtual PlatformWallet PlatformWallet { get; set; } = null!;

    public Guid? ReferenceId { get; set; }

    [MaxLength(50)]
    public string? ReferenceType { get; set; }

    // Transaction Details
    public PlatformTransactionType Type { get; set; }

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
    public string? Metadata { get; set; } // Additional data (order details, etc.)
}