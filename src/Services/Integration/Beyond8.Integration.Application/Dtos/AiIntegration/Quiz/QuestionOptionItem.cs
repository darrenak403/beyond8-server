namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz
{
    public class QuestionOptionItem
    {
        public string Id { get; set; } = null!;
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
