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
        TotalStudents = entity.TotalStudents,
        TotalRevenue = entity.TotalRevenue,
        TotalPlatformFee = entity.TotalPlatformFee,
        TotalInstructorEarnings = entity.TotalInstructorEarnings,
        TotalRefundAmount = entity.TotalRefundAmount,
        TotalPaidOut = entity.TotalPaidOut,
        PendingBalance = entity.PendingBalance,
        AvgCourseRating = entity.AvgCourseRating,
        TotalReviews = entity.TotalReviews,
        UpdatedAt = entity.UpdatedAt
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
}
