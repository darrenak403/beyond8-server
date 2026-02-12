namespace Beyond8.Common.Events.Assessment;

public record QuizAttemptsResetEvent(
    Guid QuizId,
    Guid LessonId,
    Guid StudentId,
    Guid ResetByInstructorId,
    DateTime ResetAt
);
