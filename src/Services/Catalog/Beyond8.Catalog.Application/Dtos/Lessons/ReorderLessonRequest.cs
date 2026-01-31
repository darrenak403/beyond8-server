using System.ComponentModel.DataAnnotations;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class ReorderLessonRequest
{
    public Guid LessonId { get; set; }
    public Guid NewSectionId { get; set; }
    public int NewOrderIndex { get; set; }
}