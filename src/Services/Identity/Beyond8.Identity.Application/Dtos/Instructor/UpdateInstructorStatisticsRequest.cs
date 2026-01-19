using System.ComponentModel.DataAnnotations;

public class UpdateInstructorStatisticsRequest
{
    [Range(0, int.MaxValue, ErrorMessage = "Tổng số học viên phải lớn hơn hoặc bằng 0")]
    public int TotalStudents { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Tổng số khóa học phải lớn hơn hoặc bằng 0")]
    public int TotalCourses { get; set; }

    [Range(0, 5, ErrorMessage = "Đánh giá trung bình phải từ 0 đến 5")]
    public decimal AvgRating { get; set; }
}