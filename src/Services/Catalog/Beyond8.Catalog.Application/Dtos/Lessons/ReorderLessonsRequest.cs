using Beyond8.Catalog.Application.Dtos.Lessons;

namespace Beyond8.Catalog.Application.Dtos.Lessons;

public class ReorderLessonsRequest
{
    public Guid SectionId { get; set; }
    public List<ReorderLessonRequest> Lessons { get; set; } = [];
}