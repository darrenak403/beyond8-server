using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class CreateVideoLessonRequest
{
    [Required]
    public Guid SectionId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsPreview { get; set; } = false;

    // Video-specific fields
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public bool IsDownloadable { get; set; } = false;
}