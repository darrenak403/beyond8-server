using Beyond8.Common.Data.Implements;
using Beyond8.Learning.Domain.Entities;
using Beyond8.Learning.Domain.Repositories.Interfaces;
using Beyond8.Learning.Infrastructure.Data;

namespace Beyond8.Learning.Infrastructure.Repositories.Implements;

public class CourseCertificateEligibilityConfigRepository(LearningDbContext context)
    : PostgresRepository<CourseCertificateEligibilityConfig>(context), ICourseCertificateEligibilityConfigRepository;
