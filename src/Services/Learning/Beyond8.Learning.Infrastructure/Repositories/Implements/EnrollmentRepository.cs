using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Learning.Infrastructure.Repositories.Implements;

public class EnrollmentRepository(LearningDbContext context) : PostgresRepository<Enrollment>(context), IEnrollmentRepository
{
    public async Task<int> CountActiveByCourseIdAsync(Guid courseId)
    {
        return await context.Enrollments
            .CountAsync(e => e.CourseId == courseId && e.DeletedAt == null);
    }

    public async Task<List<Guid>> GetEnrolledCourseIdsAsync(Guid userId)
    {
        return await context.Enrollments
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .Select(e => e.CourseId)
            .ToListAsync();
    }

    public async Task<List<Enrollment>> GetEnrolledCoursesAsync(Guid userId)
    {
        return await context.Enrollments
            .Where(e => e.UserId == userId && e.DeletedAt == null)
            .ToListAsync();
    }
}

