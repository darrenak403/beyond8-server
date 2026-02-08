using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Sale.Domain.Enums;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Represents an instructor's payout (withdrawal) request
/// Admin approval required before processing bank transfer
/// </summary>
public class PayoutRequest : BaseEntity
{
    // References
    public Guid InstructorId { get; set; } // Logical reference to Identity Service

    public Guid WalletId { get; set; }

    [ForeignKey(nameof(WalletId))]
    public virtual InstructorWallet InstructorWallet { get; set; } = null!;

    // Request Identification
    [Required, MaxLength(50)]
    public string RequestNumber { get; set; } = string.Empty; // Format: PR-YYYYMMDD-XXXXX

    // Amount
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    // Status
    public PayoutStatus Status { get; set; } = PayoutStatus.Requested;

    // Bank Account Information (from wallet or entered at request time)
    [Required, MaxLength(100)]
    public string BankName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string BankAccountNumber { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string BankAccountName { get; set; } = string.Empty;

    // Timestamps
    public DateTime RequestedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; } // Admin UserId who approved

    public DateTime? ProcessedAt { get; set; } // When bank transfer was completed

    public DateTime? RejectedAt { get; set; }

    public Guid? RejectedBy { get; set; } // Admin UserId who rejected

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    // External Transaction
    [MaxLength(200)]
    public string? ExternalTransactionId { get; set; } // Bank transfer transaction ID

    // Notes
    [MaxLength(1000)]
    public string? Note { get; set; } // Instructor's note or admin's processing note
}
