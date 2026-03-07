using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Application.Dtos.Progress;

public class LessonProgressResponse
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid CourseId { get; set; }

    public LessonProgressStatus Status { get; set; }
    public bool IsPassed { get; set; }
    public int LastPositionSeconds { get; set; }
    public int TotalDurationSeconds { get; set; }
    public decimal WatchPercent { get; set; }

    public int? QuizAttempts { get; set; }
    public decimal? QuizBestScore { get; set; }

    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public bool IsManuallyCompleted { get; set; }
}
