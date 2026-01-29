using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class ReorderLessonRequest
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public int NewOrderIndex { get; set; }
}