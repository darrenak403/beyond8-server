using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements;

public class AiPromptRepository(IntegrationDbContext context) : PostgresRepository<AiPrompt>(context), IAiPromptRepository
{
}
