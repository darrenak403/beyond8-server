using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class InstructorWallet : BaseEntity
{
    public Guid InstructorId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal AvailableBalance { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal PendingBalance { get; set; }

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    public virtual ICollection<TransactionLedger> Transactions { get; set; } = [];
}
