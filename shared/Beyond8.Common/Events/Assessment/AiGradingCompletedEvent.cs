namespace Beyond8.Common.Events.Assessment;

public record AiGradingCompletedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid? SectionId,
    Guid StudentId,
    decimal AiScore,
    decimal ScorePercent,
    int PassScorePercent,
    string AiFeedback,
    bool IsSuccess,
    string? ErrorMessage,
    DateTime GradedAt
);
