using System.Linq.Expressions;
using Beyond8.Assessment.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Assessment.Domain.Repositories.Interfaces;

public interface IAssignmentSubmissionRepository : IGenericRepository<AssignmentSubmission>
{
    Task<AssignmentSubmission?> FindOneWithAssignmentAsync(Expression<Func<AssignmentSubmission, bool>> expression);
    Task<IReadOnlyCollection<AssignmentSubmission>> GetAllWithAssignmentAsync(Expression<Func<AssignmentSubmission, bool>> expression);
}
