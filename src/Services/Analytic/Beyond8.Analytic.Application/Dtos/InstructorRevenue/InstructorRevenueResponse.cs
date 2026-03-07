namespace Beyond8.Analytic.Application.Dtos.InstructorRevenue;

/// <summary>
/// Dữ liệu thống kê doanh thu đầy đủ dành cho Admin/Staff.
/// Kế thừa MyRevenueResponse và bổ sung các trường tài chính nội bộ.
/// </summary>
public class InstructorRevenueResponse : MyRevenueResponse
{
    public Guid Id { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFee { get; set; }
}
