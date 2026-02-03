using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Learning.Domain.Entities;

public class LessonNote : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    [Required, MaxLength(1000)]
    public string Content { get; set; } = string.Empty;

    public int? VideoTimestampSeconds { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsPrivate { get; set; } = true;
}