using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Application.Helpers;

public static class EnrollmentProgressHelper
{
    public static bool IsCompletedOrFailed(LessonProgressStatus status) =>
        status is LessonProgressStatus.Completed or LessonProgressStatus.Failed;

    public static decimal CalculateProgressPercent(int completedCount, int totalLessons) =>
        totalLessons > 0
            ? Math.Min(100m, Math.Round((decimal)completedCount * 100 / totalLessons, 2))
            : 0m;

    public static void ApplyProgressToEnrollment(
        Enrollment enrollment,
        int completedCount,
        DateTime? completedAtFallback = null,
        Guid? lastAccessedLessonId = null,
        DateTime? lastAccessedAt = null)
    {
        enrollment.CompletedLessons = completedCount;
        enrollment.ProgressPercent = CalculateProgressPercent(completedCount, enrollment.TotalLessons);

        if (completedCount >= enrollment.TotalLessons && enrollment.TotalLessons > 0)
            enrollment.CompletedAt = enrollment.CompletedAt ?? completedAtFallback ?? DateTime.UtcNow;
        else
            enrollment.CompletedAt = null;

        if (lastAccessedLessonId.HasValue)
            enrollment.LastAccessedLessonId = lastAccessedLessonId;
        if (lastAccessedAt.HasValue)
            enrollment.LastAccessedAt = lastAccessedAt;
    }
}
