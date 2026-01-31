namespace Beyond8.Common.Events.Catalog;

public record CoursePublishedEvent(
    Guid CourseId,
    Guid InstructorId,
    DateTime Timestamp
);
