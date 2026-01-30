using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Domain.Entities;

namespace Beyond8.Catalog.Application.Mappings.LessonMappings;

public static class LessonMappings
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
            HlsVariants = lesson.HlsVariants,
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

    public static Lesson ToEntity(this CreateLessonRequest request)
    {
        return new Lesson
        {
            SectionId = request.SectionId,
            Title = request.Title,
            Description = request.Description,
            Type = request.Type,
            IsPreview = request.IsPreview,
            IsPublished = true,
            VideoOriginalUrl = request.VideoOriginalUrl,
            VideoThumbnailUrl = request.VideoThumbnailUrl,
            DurationSeconds = request.DurationSeconds,
            IsDownloadable = request.IsDownloadable,
            TextContent = request.TextContent,
            QuizId = request.QuizId,
            MinCompletionSeconds = request.MinCompletionSeconds,
            RequiredScore = request.RequiredScore
        };
    }

    public static void UpdateFrom(this Lesson lesson, UpdateLessonRequest request)
    {
        lesson.Title = request.Title;
        lesson.Description = request.Description;
        lesson.Type = request.Type;
        lesson.IsPreview = request.IsPreview;
        lesson.IsPublished = request.IsPublished;
        lesson.HlsVariants = request.HlsVariants;
        lesson.VideoOriginalUrl = request.VideoOriginalUrl;
        lesson.VideoThumbnailUrl = request.VideoThumbnailUrl;
        lesson.DurationSeconds = request.DurationSeconds;
        lesson.VideoQualities = request.VideoQualities;
        lesson.IsDownloadable = request.IsDownloadable;
        lesson.TextContent = request.TextContent;
        lesson.QuizId = request.QuizId;
        lesson.MinCompletionSeconds = request.MinCompletionSeconds;
        lesson.RequiredScore = request.RequiredScore;
    }
}