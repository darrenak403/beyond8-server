namespace Beyond8.Learning.Application.Dtos.Enrollments;

/// <summary>
/// Thống kê học viên của một giảng viên cụ thể, dùng cho internal analytics.
/// TotalStudents = số user khác nhau đã đăng ký ít nhất 1 khóa của giảng viên này.
/// </summary>
public class InstructorEnrollmentStatsResponse
{
    public int TotalStudents { get; set; }
    public int StudentsThisMonth { get; set; }
    public int StudentsLastMonth { get; set; }
}
