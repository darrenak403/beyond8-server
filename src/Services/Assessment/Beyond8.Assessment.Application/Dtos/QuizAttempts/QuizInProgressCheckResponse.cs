namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class QuizInProgressCheckResponse
{
    public bool HasInProgress { get; set; }
    public Guid? AttemptId { get; set; }
}
