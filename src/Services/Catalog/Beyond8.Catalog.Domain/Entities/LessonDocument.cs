using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Catalog.Domain.Entities;

public class LessonDocument : BaseEntity
{
    public Guid LessonId { get; set; }
    [ForeignKey(nameof(LessonId))]
    public virtual Lesson Lesson { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Required]
    public string LessonDocumentUrl { get; set; } = string.Empty;

    public bool IsDownloadable { get; set; } = false;
    public int DownloadCount { get; set; } = 0;

    public Guid? VectorDbId { get; set; }
    public bool IsIndexedInVectorDb { get; set; } = false;

}
