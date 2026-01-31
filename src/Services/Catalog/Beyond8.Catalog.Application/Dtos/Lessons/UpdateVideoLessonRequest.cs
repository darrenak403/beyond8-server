using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class UpdateVideoLessonRequest
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool? IsPreview { get; set; }
    // Video-specific fields
    public string? HlsVariants { get; set; }
    public string? VideoOriginalUrl { get; set; }
    public string? VideoThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
    public string? VideoQualities { get; set; }
    public bool? IsDownloadable { get; set; }
}