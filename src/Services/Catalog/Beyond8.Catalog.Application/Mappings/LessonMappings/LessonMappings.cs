using Beyond8.Catalog.Application.Dtos.Lessons;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Mappings.LessonMappings;

public static class LessonMappings
{
    public static LessonResponse ToResponse(this Lesson lesson)
    {
        var response = new LessonResponse
        {
            Id = lesson.Id,
            SectionId = lesson.SectionId,
            Title = lesson.Title,
            Description = lesson.Description,
            Type = lesson.Type,
            OrderIndex = lesson.OrderIndex,
            IsPreview = lesson.IsPreview,
            IsPublished = lesson.IsPublished,
            TotalViews = lesson.TotalViews,
            TotalCompletions = lesson.TotalCompletions,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt
        };

        // Only populate fields for the specific lesson type
        switch (lesson.Type)
        {
            case LessonType.Video:
                response.HlsVariants = lesson.Video?.HlsVariants;
                response.VideoOriginalUrl = lesson.Video?.VideoOriginalUrl;
                response.VideoThumbnailUrl = lesson.Video?.VideoThumbnailUrl;
                response.DurationSeconds = lesson.Video?.DurationSeconds;
                response.VideoQualities = lesson.Video?.VideoQualities;
                response.IsDownloadable = lesson.Video?.IsDownloadable ?? false;
                break;

            case LessonType.Text:
                response.TextContent = lesson.Text?.TextContent;
                break;

            case LessonType.Quiz:
                response.QuizId = lesson.Quiz?.QuizId;
                break;
        }

        return response;
    }

    // New specific mappings for individual lesson types
    public static Lesson ToEntity(this CreateVideoLessonRequest request)
    {
        return new Lesson
        {
            SectionId = request.SectionId,
            Title = request.Title,
            Description = request.Description,
            Type = LessonType.Video,
            IsPreview = request.IsPreview,
            IsPublished = true // New lessons are published by default
        };
    }

    public static Lesson ToEntity(this CreateTextLessonRequest request)
    {
        return new Lesson
        {
            SectionId = request.SectionId,
            Title = request.Title,
            Description = request.Description,
            Type = LessonType.Text,
            IsPreview = request.IsPreview,
            IsPublished = true // New lessons are published by default
        };
    }

    public static Lesson ToEntity(this CreateQuizLessonRequest request)
    {
        return new Lesson
        {
            SectionId = request.SectionId,
            Title = request.Title,
            Description = request.Description,
            Type = LessonType.Quiz,
            IsPreview = request.IsPreview,
            IsPublished = true // New lessons are published by default
        };
    }

    public static void UpdateFrom(this Lesson lesson, UpdateVideoLessonRequest request)
    {
        lesson.Title = request.Title ?? lesson.Title;
        lesson.Description = request.Description ?? lesson.Description;
        lesson.IsPreview = request.IsPreview ?? lesson.IsPreview;
        lesson.Type = LessonType.Video;
    }

    public static void UpdateFrom(this Lesson lesson, UpdateTextLessonRequest request)
    {
        lesson.Title = request.Title ?? lesson.Title;
        lesson.Description = request.Description ?? lesson.Description;
        lesson.IsPreview = request.IsPreview ?? lesson.IsPreview;
        lesson.Type = LessonType.Text;
    }

    public static void UpdateFrom(this Lesson lesson, UpdateQuizLessonRequest request)
    {
        lesson.Title = request.Title ?? lesson.Title;
        lesson.Description = request.Description ?? lesson.Description;
        lesson.IsPreview = request.IsPreview ?? lesson.IsPreview;
        lesson.Type = LessonType.Quiz;
    }

    // New entity creation methods for specific types
    public static LessonVideo ToVideoEntity(this CreateVideoLessonRequest request, Guid lessonId)
    {
        return new LessonVideo
        {
            LessonId = lessonId,
            VideoOriginalUrl = request.VideoOriginalUrl,
            VideoThumbnailUrl = request.VideoThumbnailUrl,
            DurationSeconds = request.DurationSeconds,
            IsDownloadable = request.IsDownloadable
        };
    }

    public static LessonText ToTextEntity(this CreateTextLessonRequest request, Guid lessonId)
    {
        return new LessonText
        {
            LessonId = lessonId,
            TextContent = request.Content
        };
    }

    public static LessonQuiz ToQuizEntity(this CreateQuizLessonRequest request, Guid lessonId)
    {
        return new LessonQuiz
        {
            LessonId = lessonId,
            QuizId = request.QuizId
        };
    }

    // New update methods for specific types
    public static void UpdateVideoFrom(this LessonVideo video, UpdateVideoLessonRequest request)
    {
        video.HlsVariants = request.HlsVariants ?? video.HlsVariants;
        video.VideoOriginalUrl = request.VideoOriginalUrl ?? video.VideoOriginalUrl;
        video.VideoThumbnailUrl = request.VideoThumbnailUrl ?? video.VideoThumbnailUrl;
        video.DurationSeconds = request.DurationSeconds ?? video.DurationSeconds;
        video.VideoQualities = request.VideoQualities ?? video.VideoQualities;
        video.IsDownloadable = request.IsDownloadable ?? video.IsDownloadable;
    }

    public static void UpdateTextFrom(this LessonText text, UpdateTextLessonRequest request)
    {
        text.TextContent = request.Content ?? text.TextContent;
    }

    public static void UpdateQuizFrom(this LessonQuiz quiz, UpdateQuizLessonRequest request)
    {
        quiz.QuizId = request.QuizId ?? quiz.QuizId;
    }

    // Entity creation methods for update requests (when entity doesn't exist)
    public static LessonVideo ToVideoEntity(this UpdateVideoLessonRequest request, Guid lessonId)
    {
        return new LessonVideo
        {
            LessonId = lessonId,
            HlsVariants = request.HlsVariants,
            VideoOriginalUrl = request.VideoOriginalUrl,
            VideoThumbnailUrl = request.VideoThumbnailUrl,
            DurationSeconds = request.DurationSeconds,
            VideoQualities = request.VideoQualities,
            IsDownloadable = request.IsDownloadable ?? false
        };
    }

    public static LessonText ToTextEntity(this UpdateTextLessonRequest request, Guid lessonId)
    {
        return new LessonText
        {
            LessonId = lessonId,
            TextContent = request.Content
        };
    }

    public static LessonQuiz ToQuizEntity(this UpdateQuizLessonRequest request, Guid lessonId)
    {
        return new LessonQuiz
        {
            LessonId = lessonId,
            QuizId = request.QuizId
        };
    }
}