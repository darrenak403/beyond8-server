namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz
{
    public class GenQuizResponse
    {
        public Guid CourseId { get; set; }
        public string? Query { get; set; }
        public int TotalQuestions => Easy.Count + Medium.Count + Hard.Count;

        public List<QuizQuestionDto> Easy { get; set; } = [];

        public List<QuizQuestionDto> Medium { get; set; } = [];

        public List<QuizQuestionDto> Hard { get; set; } = [];
    }
}
