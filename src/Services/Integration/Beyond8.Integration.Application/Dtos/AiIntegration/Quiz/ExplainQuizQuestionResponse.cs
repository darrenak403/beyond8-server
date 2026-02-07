namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz
{
    public class ExplainQuizQuestionResponse
    {
        public List<ExplainQuizQuestionAnswer> Answers { get; set; } = [];
    }

    public class ExplainQuizQuestionAnswer
    {
        public string Answer { get; set; } = string.Empty; //answer text
        public bool IsCorrect { get; set; } //true or false
        public string Explanation { get; set; } = string.Empty; //explanation text
    }
}