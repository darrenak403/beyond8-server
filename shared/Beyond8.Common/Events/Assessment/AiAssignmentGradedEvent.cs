namespace Beyond8.Common.Events.Assessment;

public record AiAssignmentGradedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid? SectionId,
    Guid StudentId,
    string? AssignmentTitle,
    decimal Score,
    string? AiFeedback,
    DateTime GradedAt
);
