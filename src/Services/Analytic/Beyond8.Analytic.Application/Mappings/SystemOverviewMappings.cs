using Beyond8.Analytic.Application.Dtos.SystemOverview;
using Beyond8.Analytic.Domain.Entities;

namespace Beyond8.Analytic.Application.Mappings;

public static class SystemOverviewMappings
{
    public static SystemOverviewResponse ToResponse(this AggSystemOverview entity) => new()
    {
        TotalCourses = entity.TotalCourses,
        TotalPublishedCourses = entity.TotalPublishedCourses,
        TotalEnrollments = entity.TotalEnrollments,
        TotalCompletedEnrollments = entity.TotalCompletedEnrollments,
        TotalRevenue = entity.TotalRevenue,
        TotalPlatformFee = entity.TotalPlatformFee,
        TotalInstructorEarnings = entity.TotalInstructorEarnings,
        TotalRefundAmount = entity.TotalRefundAmount,
        AvgCourseCompletionRate = entity.AvgCourseCompletionRate,
        AvgCourseRating = entity.AvgCourseRating,
        TotalReviews = entity.TotalReviews,
        UpdatedAt = entity.UpdatedAt
    };
}
