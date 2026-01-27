using Beyond8.Common.Data.Interfaces;
using Beyond8.Integration.Domain.Entities;

namespace Beyond8.Integration.Domain.Repositories.Interfaces
{
    public interface IAiPromptRepository : IGenericRepository<AiPrompt>
    {
        Task<AiPrompt?> GetActiveByNameAsync(string name);
    }
}
