namespace Beyond8.Analytic.Application.Dtos.LessonPerformance;

public class LessonPerformanceResponse
{
    public Guid Id { get; set; }
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public int TotalViews { get; set; }
    public int UniqueViewers { get; set; }
    public int TotalCompletions { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal AvgWatchPercent { get; set; }
    public int AvgWatchTimeSeconds { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
