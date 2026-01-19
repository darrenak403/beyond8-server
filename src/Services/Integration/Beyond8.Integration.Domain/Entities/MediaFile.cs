using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Domain.Entities;

[Index(nameof(UserId))]
public class MediaFile : BaseEntity
{
    public Guid UserId { get; set; }

    public StorageProvider Provider { get; set; }

    [Required]
    [MaxLength(1000)]
    public string FilePath { get; set; } = null!;

    [Required]
    [MaxLength(500)]
    public string OriginalFileName { get; set; } = null!;

    [Required]
    [MaxLength(200)]
    public string ContentType { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string Extension { get; set; } = null!;

    public long Size { get; set; }

    public FileStatus Status { get; set; } = FileStatus.Pending;

    [Column(TypeName = "jsonb")]
    public string? Metadata { get; set; }
}