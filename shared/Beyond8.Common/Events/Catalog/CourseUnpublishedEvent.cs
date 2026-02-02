namespace Beyond8.Common.Events.Catalog;

public record CourseUnpublishedEvent(
    Guid CourseId,
    Guid InstructorId,
    DateTime Timestamp
);
