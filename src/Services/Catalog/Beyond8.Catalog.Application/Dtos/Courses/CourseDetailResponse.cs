using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseDetailResponse : CourseResponse
{
    public List<SectionSummaryDto> Sections { get; set; } = [];
}