using Beyond8.Learning.Application.Dtos.Progress;
using Beyond8.Learning.Domain.Entities;

namespace Beyond8.Learning.Application.Mappings;

public static class ProgressMappings
{
    public static LessonProgressResponse ToResponse(this LessonProgress entity)
    {
        return new LessonProgressResponse
        {
            Id = entity.Id,
            LessonId = entity.LessonId,
            EnrollmentId = entity.EnrollmentId,
            CourseId = entity.CourseId,
            Status = entity.Status,
            LastPositionSeconds = entity.LastPositionSeconds,
            TotalDurationSeconds = entity.TotalDurationSeconds,
            WatchPercent = entity.WatchPercent,
            QuizAttempts = entity.QuizAttempts,
            QuizBestScore = entity.QuizBestScore,
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            LastAccessedAt = entity.LastAccessedAt,
            IsManuallyCompleted = entity.IsManuallyCompleted
        };
    }
}
