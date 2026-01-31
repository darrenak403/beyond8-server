namespace Beyond8.Assessment.Application.Dtos.Questions
{
    public class QuestionFromAiRequest
    {
        public List<QuestionRequest> Easy { get; set; } = [];

        public List<QuestionRequest> Medium { get; set; } = [];

        public List<QuestionRequest> Hard { get; set; } = [];
    }
}