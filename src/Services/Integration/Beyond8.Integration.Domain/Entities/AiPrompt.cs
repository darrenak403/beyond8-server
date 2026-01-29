using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Domain.Entities;

[Index(nameof(Category), nameof(IsActive))]
[Index(nameof(Name), nameof(Version))]
public class AiPrompt : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = null!;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public PromptCategory Category { get; set; }

    [Required]
    public string Template { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Version { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    [Column(TypeName = "jsonb")]
    public string? Variables { get; set; }

    [Column(TypeName = "jsonb")]
    public string? DefaultParameters { get; set; }

    [MaxLength(2000)]
    public string? SystemPrompt { get; set; }

    public int MaxTokens { get; set; } = 1000;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal Temperature { get; set; } = 0.7m;

    [Column(TypeName = "decimal(3, 2)")]
    public decimal TopP { get; set; } = 0.9m;

    public virtual ICollection<AiUsage> Usages { get; set; } = [];

    [MaxLength(500)]
    public string? Tags { get; set; }
}
