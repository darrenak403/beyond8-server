using Beyond8.Assessment.Application.Dtos.Questions;

namespace Beyond8.Assessment.Application.Dtos.Quizzes;

public class QuizResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public Guid? CourseId { get; set; }
    public Guid? LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassScorePercent { get; set; }
    public int TotalPoints { get; set; }
    public int MaxAttempts { get; set; }
    public bool ShuffleQuestions { get; set; }
    public bool AllowReview { get; set; }
    public bool ShowExplanation { get; set; }
    public DifficultyDistributionDto? DifficultyDistribution { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<QuestionResponse> Questions { get; set; } = [];
}
