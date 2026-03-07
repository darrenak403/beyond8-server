namespace Beyond8.Learning.Application.Dtos.CourseReview
{
    public class CreateCourseReviewRequest
    {
        public Guid CourseId { get; set; }
        public Guid EnrollmentId { get; set; }
        public int Rating { get; set; }
        public string? Review { get; set; }
        public int? ContentQuality { get; set; }
        public int? InstructorQuality { get; set; }
        public int? ValueForMoney { get; set; }
    }
}