using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Domain.Entities;

[Index(nameof(UserId), nameof(CreatedAt))]
[Index(nameof(Provider), nameof(CreatedAt))]
public class AiUsage : BaseEntity
{
    public Guid UserId { get; set; }

    public AiProvider Provider { get; set; }

    [Required]
    [MaxLength(100)]
    public string Model { get; set; } = null!;

    public AiOperation Operation { get; set; }

    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal InputCost { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal OutputCost { get; set; }

    [Column(TypeName = "decimal(10, 4)")]
    public decimal TotalCost { get; set; }

    public Guid? PromptId { get; set; }
    public virtual AiPrompt? Prompt { get; set; }

    [MaxLength(500)]
    public string? RequestSummary { get; set; }

    public int ResponseTimeMs { get; set; }

    public AiUsageStatus Status { get; set; }

    [MaxLength(1000)]
    public string? ErrorMessage { get; set; }

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
}
