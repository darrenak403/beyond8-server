using Beyond8.Assessment.Domain.Entities;
using Beyond8.Assessment.Domain.Repositories.Interfaces;
using Beyond8.Assessment.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Assessment.Infrastructure.Repositories.Implements;

public class QuestionRepository(AssessmentDbContext context) : PostgresRepository<Question>(context), IQuestionRepository
{
}
