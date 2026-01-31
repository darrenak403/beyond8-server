namespace Beyond8.Common.Events.Catalog;

public record CourseApprovedEvent(
    Guid CourseId,
    Guid InstructorId,
    string? Notes,
    DateTime Timestamp
);
