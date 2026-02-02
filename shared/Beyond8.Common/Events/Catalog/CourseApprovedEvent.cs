namespace Beyond8.Common.Events.Catalog;

public record CourseApprovedEvent(
    Guid CourseId,
    Guid InstructorId,
    string InstructorEmail,
    string InstructorName,
    string CourseName,
    string? Notes,
    DateTime Timestamp
);
