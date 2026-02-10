using System.Linq.Expressions;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Assessment.Infrastructure.Repositories.Implements;

public class AssignmentSubmissionRepository(AssessmentDbContext context) : PostgresRepository<AssignmentSubmission>(context), IAssignmentSubmissionRepository
{
    public async Task<AssignmentSubmission?> FindOneWithAssignmentAsync(Expression<Func<AssignmentSubmission, bool>> expression)
    {
        return await _dbSet
            .Include(s => s.Assignment)
            .AsNoTracking()
            .FirstOrDefaultAsync(expression);
    }

    public async Task<IReadOnlyCollection<AssignmentSubmission>> GetAllWithAssignmentAsync(Expression<Func<AssignmentSubmission, bool>> expression)
    {
        return await _dbSet
            .Include(s => s.Assignment)
            .Where(expression)
            .AsNoTracking()
            .ToListAsync();
    }
}
