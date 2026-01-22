using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements;

public class InstructorProfileRepository(IdentityDbContext context) : PostgresRepository<InstructorProfile>(context), IInstructorProfileRepository
{
    public Task<List<InstructorProfile>> GetTopInstructorsByRatingAsync(int count)
    {
        throw new NotImplementedException();
    }

    public Task<(List<InstructorProfile> Profiles, int TotalCount)> GetVerifiedInstructorsAsync(int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }

    public Task<(List<InstructorProfile> Profiles, int TotalCount)> SearchInstructorsAsync(string? searchTerm, List<string>? expertiseAreas, int pageNumber, int pageSize)
    {
        throw new NotImplementedException();
    }
}
