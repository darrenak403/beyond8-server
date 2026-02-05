using Beyond8.Common.Data.Interfaces;
using Beyond8.Learning.Domain.Entities;

namespace Beyond8.Learning.Domain.Repositories.Interfaces;

public interface ICourseReviewRepository : IGenericRepository<CourseReview>
{
    Task<bool> IsCourseReviewedAsync(Guid courseId, Guid userId);
    Task<bool> IsEnrollmentReviewedAsync(Guid enrollmentId, Guid userId);
}
