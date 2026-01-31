using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class QuizAttemptSummaryResponse
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public decimal? ScorePercent { get; set; }
    public bool? IsPassed { get; set; }
    public int TimeSpentSeconds { get; set; }
    public QuizAttemptStatus Status { get; set; }
}

public class UserQuizAttemptsResponse
{
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int MaxAttempts { get; set; }
    public int UsedAttempts { get; set; }
    public int RemainingAttempts { get; set; }
    public decimal? BestScore { get; set; }
    public decimal? LatestScore { get; set; }
    public List<QuizAttemptSummaryResponse> Attempts { get; set; } = [];
}
