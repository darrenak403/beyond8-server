using Beyond8.Learning.Domain.Enums;

namespace Beyond8.Learning.Application.Dtos.Enrollments;

public class EnrollmentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnailUrl { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public decimal PricePaid { get; set; }
    public EnrollmentStatus Status { get; set; }
    public decimal ProgressPercent { get; set; }
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? LastAccessedAt { get; set; }
    public Guid? LastAccessedLessonId { get; set; }
}
