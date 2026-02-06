namespace Beyond8.Learning.Application.Dtos.Enrollments
{
    public class EnrollmentSimpleResponse
    {
        public Guid Id { get; set; }
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? CourseThumbnailUrl { get; set; }
        public string? CourseSlug { get; set; } = string.Empty;
        public Guid InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal ProgressPercent { get; set; } = 0;
    }
}