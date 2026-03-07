using Beyond8.Common.Data.Implements;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;

namespace Beyond8.Learning.Infrastructure.Repositories.Implements;

public class CourseReviewRepository(LearningDbContext context) : PostgresRepository<CourseReview>(context), ICourseReviewRepository
{
    public async Task<bool> IsCourseReviewedAsync(Guid courseId, Guid userId)
    {
        return await CountAsync(cr => cr.CourseId == courseId && cr.UserId == userId && cr.DeletedAt == null) > 0;
    }

    public async Task<bool> IsEnrollmentReviewedAsync(Guid enrollmentId, Guid userId)
    {
        return await CountAsync(cr => cr.EnrollmentId == enrollmentId && cr.UserId == userId && cr.DeletedAt == null) > 0;
    }
}