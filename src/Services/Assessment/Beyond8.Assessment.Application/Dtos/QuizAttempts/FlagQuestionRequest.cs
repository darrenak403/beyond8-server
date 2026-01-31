namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class FlagQuestionRequest
{
    public Guid QuestionId { get; set; }
    public bool IsFlagged { get; set; }
}
