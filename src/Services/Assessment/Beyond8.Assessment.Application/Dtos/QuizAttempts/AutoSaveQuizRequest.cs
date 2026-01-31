namespace Beyond8.Assessment.Application.Dtos.QuizAttempts;

public class AutoSaveQuizRequest
{
    public Dictionary<string, List<string>> Answers { get; set; } = [];

    public int TimeSpentSeconds { get; set; }

    public List<Guid> FlaggedQuestions { get; set; } = [];
}
