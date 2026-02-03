using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;
using Beyond8.Common.Data.Implements;

namespace Beyond8.Learning.Infrastructure.Repositories.Implements;

public class EnrollmentRepository(LearningDbContext context) : PostgresRepository<Enrollment>(context), IEnrollmentRepository
{
}
