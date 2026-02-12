using Beyond8.Analytic.Application.Dtos.LessonPerformance;
using Beyond8.Analytic.Domain.Entities;

namespace Beyond8.Analytic.Application.Mappings;

public static class LessonPerformanceMappings
{
    public static LessonPerformanceResponse ToResponse(this AggLessonPerformance entity) => new()
    {
        Id = entity.Id,
        LessonId = entity.LessonId,
        LessonTitle = entity.LessonTitle,
        CourseId = entity.CourseId,
        TotalViews = entity.TotalViews,
        UniqueViewers = entity.UniqueViewers,
        TotalCompletions = entity.TotalCompletions,
        CompletionRate = entity.CompletionRate,
        AvgWatchPercent = entity.AvgWatchPercent,
        AvgWatchTimeSeconds = entity.AvgWatchTimeSeconds,
        UpdatedAt = entity.UpdatedAt
    };
}
