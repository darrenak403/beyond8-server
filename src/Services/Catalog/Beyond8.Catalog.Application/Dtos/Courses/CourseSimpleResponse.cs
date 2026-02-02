using Beyond8.Catalog.Domain.Enums;

namespace Beyond8.Catalog.Application.Dtos.Courses
{
    public class CourseSimpleResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ShortDescription { get; set; } = string.Empty;
        public string ThumbnailUrl { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public CourseLevel Level { get; set; }
        public int TotalStudents { get; set; }
        public int TotalDurationMinutes { get; set; }
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0;
        public decimal? AvgRating { get; set; }
        public int TotalReviews { get; set; }
    }
}