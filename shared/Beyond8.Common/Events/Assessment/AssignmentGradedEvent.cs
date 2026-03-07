namespace Beyond8.Common.Events.Assessment;

public record AssignmentGradedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    string? AssignmentTitle,
    Guid? SectionId,
    Guid StudentId,
    decimal Score,
    decimal ScorePercent,
    int PassScorePercent,
    DateTime GradedAt,
    Guid GradedBy
);
