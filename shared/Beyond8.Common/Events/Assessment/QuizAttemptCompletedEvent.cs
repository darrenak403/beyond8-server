namespace Beyond8.Common.Events.Assessment;

public record QuizAttemptCompletedEvent(
    Guid LessonId,
    string LessonTitle,
    Guid StudentId,
    decimal ScorePercent,
    decimal TotalScore,
    decimal MaxScore,
    bool IsPassed,
    Guid AttemptId,
    Guid QuizId,
    DateTime CompletedAt,
    Guid? CourseId = null,
    Guid? InstructorId = null
);
