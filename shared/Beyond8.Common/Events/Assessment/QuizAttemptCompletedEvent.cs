namespace Beyond8.Common.Events.Assessment;

public record QuizAttemptCompletedEvent(
    Guid LessonId,
    Guid StudentId,
    decimal ScorePercent,
    decimal TotalScore,
    decimal MaxScore,
    bool IsPassed,
    Guid AttemptId,
    Guid QuizId,
    DateTime CompletedAt
);
