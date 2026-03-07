using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Analytic.Domain.Entities;

public class AggAiUsageDaily : BaseEntity
{
    public DateOnly SnapshotDate { get; set; }

    [Required, MaxLength(100)]
    public string Model { get; set; } = string.Empty;
    public int Provider { get; set; }
    public int TotalInputTokens { get; set; }
    public int TotalOutputTokens { get; set; }
    public int TotalTokens { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal TotalInputCost { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal TotalOutputCost { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal TotalCost { get; set; }

    public int UsageCount { get; set; }
}
