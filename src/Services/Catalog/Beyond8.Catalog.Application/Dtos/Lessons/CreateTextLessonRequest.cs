using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class CreateTextLessonRequest
{
    [Required]
    public Guid SectionId { get; set; }

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool IsPreview { get; set; } = false;

    // Text-specific fields
    [Required, MaxLength(50000)]
    public string Content { get; set; } = string.Empty;
}