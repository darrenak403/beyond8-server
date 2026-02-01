using Beyond8.Catalog.Application.Dtos.Lessons;

namespace Beyond8.Catalog.Application.Dtos.Sections
{
    public class SectionDetailResponse : SectionResponse
    {
        public List<LessonDetailResponse> Lessons { get; set; } = [];
    }
}