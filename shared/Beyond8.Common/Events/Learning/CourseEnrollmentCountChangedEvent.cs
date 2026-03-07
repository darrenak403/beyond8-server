namespace Beyond8.Common.Events.Learning;

public record CourseEnrollmentCountChangedEvent(
    Guid CourseId,
    int TotalStudents,
    DateTime Timestamp,
    Guid? InstructorId = null,
    int? Delta = null
);
