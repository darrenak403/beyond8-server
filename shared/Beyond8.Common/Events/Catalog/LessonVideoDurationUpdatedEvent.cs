namespace Beyond8.Common.Events.Catalog;

public record LessonVideoDurationUpdatedEvent(
    Guid LessonId,
    Guid CourseId,
    int DurationSeconds
);
