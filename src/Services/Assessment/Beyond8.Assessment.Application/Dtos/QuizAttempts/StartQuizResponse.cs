using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class StartQuizResponse
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public string? QuizDescription { get; set; }
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int TotalQuestions { get; set; }
    public int TotalPoints { get; set; }
    public int PassScorePercent { get; set; }
    public List<QuizQuestionForStudentResponse> Questions { get; set; } = [];
}

public class QuizQuestionForStudentResponse
{
    public Guid QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Points { get; set; }
    public List<QuestionOptionForStudentResponse> Options { get; set; } = [];
}

public class QuestionOptionForStudentResponse
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
