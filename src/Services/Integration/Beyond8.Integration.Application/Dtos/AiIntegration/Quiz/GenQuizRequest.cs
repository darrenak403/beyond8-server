namespace Beyond8.Integration.Application.Dtos.AiIntegration.Quiz
{
    public class GenQuizRequest
    {
        public Guid CourseId { get; set; }

        public Guid? LessonId { get; set; }

        public string? Query { get; set; }

        public int TotalCount { get; set; } = 10;

        public int MaxPoints { get; set; } = 100;

        public DifficultyDistribution? Distribution { get; set; }

        public int TopK { get; set; } = 20;
    }

    public class DifficultyDistribution
    {
        public int EasyPercent { get; set; } = 30; //%

        public int MediumPercent { get; set; } = 50;

        public int HardPercent { get; set; } = 20;
    }
}
