using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Lessons;


public class LessonSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public LessonType Type { get; set; }
    public int Order { get; set; }
    public bool IsPreview { get; set; }

    public int? DurationSeconds { get; set; }
    public string? VideoThumbnailUrl { get; set; }

    public Guid? QuizId { get; set; }

    public bool HasTextContent { get; set; }
}
