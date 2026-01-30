namespace Beyond8.Assessment.Application.Dtos.Questions
{
    public class QuestionFromAiRequest
    {
        public Guid CourseId { get; set; }
        public string? Query { get; set; }
        public int TotalQuestions => Easy.Count + Medium.Count + Hard.Count;

        public List<QuestionRequest> Easy { get; set; } = [];

        public List<QuestionRequest> Medium { get; set; } = [];

        public List<QuestionRequest> Hard { get; set; } = [];
    }
}