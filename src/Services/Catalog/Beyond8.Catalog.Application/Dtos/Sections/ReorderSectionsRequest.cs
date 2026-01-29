using Beyond8.Catalog.Application.Dtos.Sections;

namespace Beyond8.Catalog.Application.Dtos.Sections;

public class ReorderSectionsRequest
{
    public Guid CourseId { get; set; }
    public List<ReorderSectionRequest> Sections { get; set; } = [];
}