namespace Beyond8.Catalog.Application.Dtos.Courses;

public class CourseAnalyticsResponse
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;

    // Enrollment metrics
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal CompletionRate { get; set; }

    // Revenue metrics
    public decimal TotalRevenue { get; set; }
    public decimal AverageRevenuePerStudent { get; set; }

    // Rating metrics
    public decimal AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarReviews { get; set; }
    public int FourStarReviews { get; set; }
    public int ThreeStarReviews { get; set; }
    public int TwoStarReviews { get; set; }
    public int OneStarReviews { get; set; }

    // Content engagement
    public decimal AverageWatchTime { get; set; }
    public decimal AverageCompletionTime { get; set; }

    // Time-based metrics
    public List<MonthlyEnrollmentResponse> MonthlyEnrollments { get; set; } = [];
    public List<MonthlyRevenueResponse> MonthlyRevenue { get; set; } = [];
}

public class MonthlyEnrollmentResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int Enrollments { get; set; }
}

public class MonthlyRevenueResponse
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
}