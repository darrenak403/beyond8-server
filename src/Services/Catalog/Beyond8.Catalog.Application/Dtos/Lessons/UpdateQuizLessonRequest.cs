using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class UpdateQuizLessonRequest
{
    [Required]
    public Guid Id { get; set; }

    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public bool? IsPreview { get; set; }
    public bool? IsPublished { get; set; }

    // Quiz-specific fields
    public Guid? QuizId { get; set; }
}