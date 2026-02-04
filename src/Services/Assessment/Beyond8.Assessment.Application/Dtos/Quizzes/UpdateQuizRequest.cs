namespace Beyond8.Assessment.Application.Dtos.Quizzes
{
    public class UpdateQuizRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int PassScorePercent { get; set; } = 60;
        public int TotalPoints { get; set; } = 100;
        public int MaxAttempts { get; set; } = 1;
        public bool ShuffleQuestions { get; set; } = true;
        public bool AllowReview { get; set; } = true;
        public bool ShowExplanation { get; set; } = true;
        public List<Guid> QuestionIds { get; set; } = [];
        public DifficultyDistributionDto? DifficultyDistribution { get; set; }
    }
}