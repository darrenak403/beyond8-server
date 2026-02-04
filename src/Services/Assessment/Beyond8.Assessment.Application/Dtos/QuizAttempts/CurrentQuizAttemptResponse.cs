namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class CurrentQuizAttemptResponse
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

    public Dictionary<string, List<string>> SavedAnswers { get; set; } = [];

    public int TimeSpentSeconds { get; set; }
    public List<Guid> FlaggedQuestions { get; set; } = [];
}
