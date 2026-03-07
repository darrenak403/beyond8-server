namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz;

public class ExplainQuizQuestionRequest
{
    public string Content { get; set; } = string.Empty;
    public List<ExplainQuizQuestionOptionItem> Options { get; set; } = [];
}

public class ExplainQuizQuestionOptionItem
{
    public string Id { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}