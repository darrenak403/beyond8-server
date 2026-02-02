namespace Beyond8.Common.Events.Catalog;

public record CoursePublishedEvent(
    Guid CourseId,
    Guid InstructorId,
    string InstructorEmail,
    string InstructorName,
    string CourseName,
    string CourseUrl,
    DateTime Timestamp
);
