using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements
{
    public class AiPromptRepository(IntegrationDbContext context) : PostgresRepository<AiPrompt>(context), IAiPromptRepository
    {
        public async Task<AiPrompt?> GetActiveByNameAsync(string name) =>
            await _dbSet
                .Where(p => p.Name == name && p.IsActive)
                .OrderByDescending(p => p.Version)
                .FirstOrDefaultAsync();
    }
}
