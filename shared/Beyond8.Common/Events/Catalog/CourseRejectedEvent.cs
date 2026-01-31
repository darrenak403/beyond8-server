namespace Beyond8.Common.Events.Catalog;

public record CourseRejectedEvent(
    Guid CourseId,
    Guid InstructorId,
    string? Reason,
    DateTime Timestamp
);
