namespace Beyond8.Common.Events.Assessment;

public record AiGradingCompletedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    decimal AiScore,
    string AiFeedback,
    bool IsSuccess,
    string? ErrorMessage,
    DateTime GradedAt
);
