namespace Beyond8.Common.Events.Assessment;

public record AssignmentSubmissionsResetEvent(
    Guid AssignmentId,
    Guid SectionId,
    Guid StudentId,
    Guid ResetByInstructorId,
    DateTime ResetAt
);
