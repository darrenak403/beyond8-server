namespace Beyond8.Common.Events.Catalog;

public record CourseSubmittedForApprovalEvent(
    Guid CourseId,
    Guid InstructorId,
    string InstructorName,
    string CourseTitle,
    DateTime Timestamp
);
