using Beyond8.Assessment.Domain.Entities;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Assessment.Domain.Repositories.Interfaces;

public interface IQuestionRepository : IGenericRepository<Question>
{
    Task AddRangeAsync(List<Question> questions);

    Task<(List<Question> Items, int TotalCount)> GetPagedByInstructorAsync(
        Guid instructorId,
        int pageNumber,
        int pageSize,
        string? tag = null,
        bool orderByDescending = true);

    Task<IReadOnlyList<(string Tag, int Count)>> GetTagCountsByInstructorAsync(Guid instructorId);
}
