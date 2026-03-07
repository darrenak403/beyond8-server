using Beyond8.Analytic.Application.Dtos.InstructorRevenue;
using Beyond8.Analytic.Domain.Entities;

namespace Beyond8.Analytic.Application.Mappings;

public static class InstructorRevenueMappings
{
    public static InstructorRevenueResponse ToResponse(this AggInstructorRevenue entity) => new()
    {
        Id = entity.Id,
        InstructorId = entity.InstructorId,
        InstructorName = entity.InstructorName,
        SnapshotDate = entity.SnapshotDate,
        UpdatedAt = entity.UpdatedAt,
        TotalRevenue = entity.TotalRevenue,
        TotalPlatformFee = entity.TotalPlatformFee,
        Courses = new CourseStatsGroup
        {
            Total = entity.TotalCourses,
            Draft = entity.DraftCourses,
            PendingApproval = entity.PendingApprovalCourses,
            Approved = entity.ApprovedCourses,
            Published = entity.PublishedCourses,
            Rejected = entity.RejectedCourses,
            Archived = entity.ArchivedCourses,
            Suspended = entity.SuspendedCourses,
        },
        Students = new StudentStatsGroup
        {
            Total = entity.TotalStudents,
        },
        Revenue = new RevenueStatsGroup
        {
            TotalEarnings = entity.TotalInstructorEarnings,
            AvailableBalance = entity.AvailableBalance,
        },
        Rating = new RatingGroup
        {
            Average = entity.AvgCourseRating,
            TotalReviews = entity.TotalReviews,
        },
    };

    public static TopInstructorResponse ToTopResponse(this AggInstructorRevenue entity) => new()
    {
        InstructorId = entity.InstructorId,
        InstructorName = entity.InstructorName,
        TotalStudents = entity.TotalStudents,
        TotalRevenue = entity.TotalRevenue,
        AvgCourseRating = entity.AvgCourseRating,
        TotalCourses = entity.TotalCourses
    };

    public static MyRevenueResponse ToMyResponse(this AggInstructorRevenue entity) => new()
    {
        InstructorId = entity.InstructorId,
        InstructorName = entity.InstructorName,
        SnapshotDate = entity.SnapshotDate,
        UpdatedAt = entity.UpdatedAt,
        Courses = new CourseStatsGroup
        {
            Total = entity.TotalCourses,
            Draft = entity.DraftCourses,
            PendingApproval = entity.PendingApprovalCourses,
            Approved = entity.ApprovedCourses,
            Published = entity.PublishedCourses,
            Rejected = entity.RejectedCourses,
            Archived = entity.ArchivedCourses,
            Suspended = entity.SuspendedCourses,
        },
        Students = new StudentStatsGroup
        {
            Total = entity.TotalStudents,
        },
        Revenue = new RevenueStatsGroup
        {
            TotalEarnings = entity.TotalInstructorEarnings,
            AvailableBalance = entity.AvailableBalance,
        },
        Rating = new RatingGroup
        {
            Average = entity.AvgCourseRating,
            TotalReviews = entity.TotalReviews,
        },
    };
}
