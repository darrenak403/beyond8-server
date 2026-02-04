namespace Beyond8.Common.Events.Assessment;

public record AssignmentSubmittedEvent(
    Guid SubmissionId,
    Guid AssignmentId,
    Guid StudentId,
    string AssignmentTitle,
    string AssignmentDescription,
    string? TextContent,
    List<string>? FileUrls,
    string? RubricUrl,
    int TotalPoints,
    DateTime SubmittedAt
);
