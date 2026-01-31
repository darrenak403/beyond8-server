namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz
{
    public class QuizQuestionDto
    {
        public string Content { get; set; } = null!;

        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;

        public List<QuestionOptionItem> Options { get; set; } = [];

        public string? Explanation { get; set; }

        public List<string> Tags { get; set; } = [];

        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;

        public decimal Points { get; set; } = 1.0m;
    }
}
