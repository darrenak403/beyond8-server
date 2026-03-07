namespace Beyond8.Common.Events.Catalog;

public record CourseCreatedEvent(
    Guid CourseId,
    Guid InstructorId,
    string InstructorName,
    string CourseTitle,
    DateTime Timestamp
);
