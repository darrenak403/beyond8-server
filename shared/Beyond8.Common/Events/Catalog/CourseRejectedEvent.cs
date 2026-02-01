namespace Beyond8.Common.Events.Catalog;

public record CourseRejectedEvent(
    Guid CourseId,
    Guid InstructorId,
    string InstructorEmail,
    string InstructorName,
    string CourseName,
    string? Reason,
    DateTime Timestamp
);
