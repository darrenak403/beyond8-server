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
        TotalCourses = entity.TotalCourses,
        DraftCourses = entity.DraftCourses,
        PendingApprovalCourses = entity.PendingApprovalCourses,
        ApprovedCourses = entity.ApprovedCourses,
        PublishedCourses = entity.PublishedCourses,
        RejectedCourses = entity.RejectedCourses,
        ArchivedCourses = entity.ArchivedCourses,
        SuspendedCourses = entity.SuspendedCourses,
        TotalStudents = entity.TotalStudents,
        TotalInstructorEarnings = entity.TotalInstructorEarnings,
        AvailableBalance = entity.AvailableBalance,
        AvgCourseRating = entity.AvgCourseRating,
        TotalReviews = entity.TotalReviews,
        SnapshotDate = entity.SnapshotDate,
        UpdatedAt = entity.UpdatedAt,
        TotalRevenue = entity.TotalRevenue,
        TotalPlatformFee = entity.TotalPlatformFee
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
        TotalCourses = entity.TotalCourses,
        DraftCourses = entity.DraftCourses,
        PendingApprovalCourses = entity.PendingApprovalCourses,
        ApprovedCourses = entity.ApprovedCourses,
        PublishedCourses = entity.PublishedCourses,
        RejectedCourses = entity.RejectedCourses,
        ArchivedCourses = entity.ArchivedCourses,
        SuspendedCourses = entity.SuspendedCourses,
        TotalStudents = entity.TotalStudents,
        TotalInstructorEarnings = entity.TotalInstructorEarnings,
        AvailableBalance = entity.AvailableBalance,
        AvgCourseRating = entity.AvgCourseRating,
        TotalReviews = entity.TotalReviews,
        SnapshotDate = entity.SnapshotDate,
        UpdatedAt = entity.UpdatedAt
    };
}
