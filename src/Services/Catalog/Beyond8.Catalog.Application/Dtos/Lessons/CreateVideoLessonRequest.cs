namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class CreateVideoLessonRequest
{
    public Guid SectionId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsPreview { get; set; } = false;
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsDownloadable { get; set; } = false;
}