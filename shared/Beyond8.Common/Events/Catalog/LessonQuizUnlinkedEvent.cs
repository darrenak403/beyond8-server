namespace Beyond8.Common.Events.Catalog;

public record LessonQuizUnlinkedEvent(Guid LessonId, Guid QuizId);
