namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class SubmitQuizRequest
{
    public Dictionary<string, List<string>> Answers { get; set; } = [];

    public int TimeSpentSeconds { get; set; }
}
