namespace Beyond8.Assessment.Domain.JSONFields
{
    public class QuestionOptionItem
    {
        public string Id { get; set; } = null!;
        public string Text { get; set; } = null!;
        public bool IsCorrect { get; set; }
    }
}
