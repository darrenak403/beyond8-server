using System;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;
using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Domain.Entities;

public class LessonProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public Guid CourseId { get; set; }
    public Guid EnrollmentId { get; set; }

    [ForeignKey(nameof(EnrollmentId))]
    public virtual Enrollment Enrollment { get; set; } = null!;

    public LessonProgressStatus Status { get; set; } = LessonProgressStatus.NotStarted;

    public int LastPositionSeconds { get; set; } = 0;
    public int TotalDurationSeconds { get; set; } = 0;

    [Column(TypeName = "decimal(5, 2)")]
    public decimal WatchPercent { get; set; } = 0;

    public int? QuizAttempts { get; set; }

    [Column(TypeName = "decimal(5, 2)")]
    public decimal? QuizBestScore { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }

    public bool IsManuallyCompleted { get; set; } = false;
}