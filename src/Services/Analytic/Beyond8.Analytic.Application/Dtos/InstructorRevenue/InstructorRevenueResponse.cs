namespace Beyond8.Analytic.Application.Dtos.InstructorRevenue;

public class InstructorRevenueResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFee { get; set; }
    public decimal TotalInstructorEarnings { get; set; }
    public decimal TotalRefundAmount { get; set; }
    public decimal TotalPaidOut { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvgCourseRating { get; set; }
    public int TotalReviews { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
