namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class LessonVideoResponse
{
    public Guid LessonId { get; set; }
    public string? HlsVariants { get; set; }
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string? VideoQualities { get; set; }
    public bool IsDownloadable { get; set; }
}
