using Beyond8.Assessment.Domain.Enums;
using Beyond8.Assessment.Domain.JSONFields;

namespace Beyond8.Assessment.Application.Dtos.Questions
{
    public class QuestionResponse
    {
        public Guid Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public QuestionType Type { get; set; } = QuestionType.MultipleChoice;
        public List<QuestionOptionItem> Options { get; set; } = [];
        public string? Explanation { get; set; }
        public List<string> Tags { get; set; } = [];
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Medium;
        public decimal Points { get; set; } = 1.0m;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}