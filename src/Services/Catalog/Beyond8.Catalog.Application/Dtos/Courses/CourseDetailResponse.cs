using Beyond8.Catalog.Application.Dtos.Sections;

namespace Beyond8.Catalog.Application.Dtos.Courses
{
    public class CourseDetailResponse : CourseResponse
    {
        public List<SectionDetailResponse> Sections { get; set; } = [];
    }
}