using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.JSONFields;

namespace Beyond8.Identity.Application.Dtos.InstructorProfiles;

/// <summary>
/// DTO trả về thông tin hồ sơ giảng viên
/// </summary>
public class InstructorProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    // Thông tin user
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    // Thông tin profile
    public string? Bio { get; set; }
    public string? Headline { get; set; }
    public List<string>? ExpertiseAreas { get; set; }
    public List<EducationInfo>? Education { get; set; }
    public List<WorkInfo>? WorkExperience { get; set; }
    public SocialInfo? SocialLinks { get; set; }
    public List<CertificateInfo>? Certificates { get; set; }

    // Trạng thái xác thực
    public VerificationStatus VerificationStatus { get; set; }
    public string? VerificationNotes { get; set; }
    public DateTime? VerifiedAt { get; set; }

    // Thống kê
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal? AvgRating { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO trả về thông tin hồ sơ giảng viên dạng compact (cho danh sách)
/// </summary>
public class InstructorProfileSummaryResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Headline { get; set; }
    public List<string>? ExpertiseAreas { get; set; }
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public decimal? AvgRating { get; set; }
    public VerificationStatus VerificationStatus { get; set; }
}

/// <summary>
/// DTO cho Dashboard giảng viên (Luồng 5 - Step 3)
/// </summary>
public class InstructorDashboardResponse
{
    public Guid InstructorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }

    // Thống kê chung
    public int TotalCourses { get; set; }
    public int PublishedCourses { get; set; }
    public int DraftCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal AvgRating { get; set; }
    public int TotalReviews { get; set; }

    // Doanh thu (Từ Analytics Service)
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }

    // Top khóa học bán chạy
    public List<TopCourseItem> TopCourses { get; set; } = new();

    // Hoạt động gần đây
    public DateTime? LastLoginAt { get; set; }
    public DateTime? LastCoursePublishedAt { get; set; }
}

/// <summary>
/// DTO cho top khóa học trong dashboard
/// </summary>
public class TopCourseItem
{
    public Guid CourseId { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AvgRating { get; set; }
    public int TotalReviews { get; set; }
}

/// <summary>
/// DTO cho thống kê giảng viên (Admin view)
/// </summary>
public class InstructorStatisticsResponse
{
    public Guid InstructorId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public int TotalCourses { get; set; }
    public int TotalStudents { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AvgRating { get; set; }

    public DateTime JoinedAt { get; set; }
    public DateTime? LastActiveAt { get; set; }

    public VerificationStatus VerificationStatus { get; set; }
}