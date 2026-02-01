
using Beyond8.Catalog.Application.Dtos.Sections;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseSummaryResponse : CourseResponse
{
    public List<SectionSummaryResponse> Sections { get; set; } = [];
}   