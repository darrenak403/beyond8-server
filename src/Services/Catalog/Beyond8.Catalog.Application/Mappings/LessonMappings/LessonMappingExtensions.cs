using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Application.Mappings.LessonMappings;

public static class LessonMappingExtensions
{
    public static LessonResponse ToResponse(this Lesson lesson)
    {
        return new LessonResponse
        {
            Id = lesson.Id,
            SectionId = lesson.SectionId,
            Title = lesson.Title,
            Description = lesson.Description,
            Type = lesson.Type,
            OrderIndex = lesson.OrderIndex,
            IsPreview = lesson.IsPreview,
            IsPublished = lesson.IsPublished,
            VideoHlsUrl = lesson.VideoHlsUrl,
            VideoOriginalUrl = lesson.VideoOriginalUrl,
            VideoThumbnailUrl = lesson.VideoThumbnailUrl,
            DurationSeconds = lesson.DurationSeconds,
            VideoQualities = lesson.VideoQualities,
            IsDownloadable = lesson.IsDownloadable,
            TextContent = lesson.TextContent,
            QuizId = lesson.QuizId,
            MinCompletionSeconds = lesson.MinCompletionSeconds,
            RequiredScore = lesson.RequiredScore,
            TotalViews = lesson.TotalViews,
            TotalCompletions = lesson.TotalCompletions,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };
    }
}