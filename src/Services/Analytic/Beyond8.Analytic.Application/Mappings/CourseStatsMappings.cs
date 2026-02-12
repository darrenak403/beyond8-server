using Beyond8.Analytic.Application.Dtos.CourseStats;
using Beyond8.Analytic.Domain.Entities;

namespace Beyond8.Analytic.Application.Mappings;

public static class CourseStatsMappings
{
    public static CourseStatsResponse ToResponse(this AggCourseStats entity) => new()
    {
        Id = entity.Id,
        CourseId = entity.CourseId,
        CourseTitle = entity.CourseTitle,
        InstructorId = entity.InstructorId,
        InstructorName = entity.InstructorName,
        TotalStudents = entity.TotalStudents,
        TotalCompletedStudents = entity.TotalCompletedStudents,
        CompletionRate = entity.CompletionRate,
        AvgRating = entity.AvgRating,
        TotalReviews = entity.TotalReviews,
        TotalRatings = entity.TotalRatings,
        TotalRevenue = entity.TotalRevenue,
        TotalRefundAmount = entity.TotalRefundAmount,
        NetRevenue = entity.NetRevenue,
        TotalViews = entity.TotalViews,
        AvgWatchTime = entity.AvgWatchTime,
        UpdatedAt = entity.UpdatedAt
    };

    public static TopCourseResponse ToTopResponse(this AggCourseStats entity) => new()
    {
        CourseId = entity.CourseId,
        CourseTitle = entity.CourseTitle,
        InstructorName = entity.InstructorName,
        TotalStudents = entity.TotalStudents,
        TotalRevenue = entity.TotalRevenue,
        AvgRating = entity.AvgRating
    };
}
