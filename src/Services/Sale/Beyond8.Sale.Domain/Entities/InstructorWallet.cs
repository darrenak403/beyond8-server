using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class InstructorWallet : BaseEntity
{
    public Guid InstructorId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal AvailableBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal HoldBalance { get; set; } = 0;

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalEarnings { get; set; } = 0;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalWithdrawn { get; set; } = 0;

    // Refund Statistics (TODO: Implement refund logic later)
    // [Column(TypeName = "decimal(18, 2)")]
    // public decimal TotalRefunded { get; set; } = 0;

    // Timestamps
    public DateTime? LastPayoutAt { get; set; }

    public bool IsActive { get; set; } = true;

    // Bank Account Information (Encrypted in production)
    [MaxLength(2000)]
    public string? BankAccountInfo { get; set; } // JSON: { bankName, accountNumber, accountName }

    // Navigation Properties
    public virtual ICollection<TransactionLedger> Transactions { get; set; } = [];
    public virtual ICollection<PayoutRequest> PayoutRequests { get; set; } = [];
}
