using Beyond8.Assessment.Domain.Enums;

namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class QuizResultResponse
{
    public Guid AttemptId { get; set; }
    public Guid QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public int AttemptNumber { get; set; }

    public DateTime StartedAt { get; set; }
    public DateTime SubmittedAt { get; set; }
    public int TimeSpentSeconds { get; set; }

    public decimal Score { get; set; }
    public decimal ScorePercent { get; set; }
    public int TotalPoints { get; set; }
    public int PassScorePercent { get; set; }
    public bool IsPassed { get; set; }

    public int TotalQuestions { get; set; }
    public int CorrectAnswers { get; set; }
    public int WrongAnswers { get; set; }

    public QuizAttemptStatus Status { get; set; }

    public List<QuestionResultResponse>? QuestionResults { get; set; }
}

public class QuestionResultResponse
{
    public Guid QuestionId { get; set; }
    public int OrderIndex { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public decimal Points { get; set; }
    public decimal EarnedPoints { get; set; }
    public bool IsCorrect { get; set; }

    public List<string> SelectedOptions { get; set; } = [];

    public List<string> CorrectOptions { get; set; } = [];

    public List<QuestionOptionResultResponse> Options { get; set; } = [];

    public string? Explanation { get; set; }
}

public class QuestionOptionResultResponse
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public bool IsSelected { get; set; }
}
