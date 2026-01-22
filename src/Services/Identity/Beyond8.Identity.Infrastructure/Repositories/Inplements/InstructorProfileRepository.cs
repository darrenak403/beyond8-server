using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
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

    public async Task<(List<InstructorProfile> Items, int TotalCount)> SearchInstructorsPagedAsync(
    int pageNumber,
    int pageSize,
    string? email,
    string? fullName,
    string? phoneNumber,
    string? bio,
    string? headLine, // Lưu ý: tham số là headLine
    string? expertiseArea,
    string? schoolName,
    string? companyName,
    bool? isDescending)
    {
        var query = AsQueryable()
            .Include(ip => ip.User)
            .Where(ip => ip.DeletedAt == null && ip.VerificationStatus != VerificationStatus.Hidden);

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(ip => ip.User!.Email.Contains(email));

        if (!string.IsNullOrWhiteSpace(fullName))
            query = query.Where(ip => ip.User!.FullName.Contains(fullName));

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            query = query.Where(ip => ip.User!.PhoneNumber != null && ip.User.PhoneNumber.Contains(phoneNumber));

        if (!string.IsNullOrWhiteSpace(bio))
            query = query.Where(ip => ip.Bio != null && ip.Bio.Contains(bio));

        if (!string.IsNullOrWhiteSpace(headLine))
            query = query.Where(ip => ip.Headline != null && ip.Headline.Contains(headLine));

        if (!string.IsNullOrWhiteSpace(expertiseArea))
            query = query.Where(ip => ip.ExpertiseAreas != null && ip.ExpertiseAreas.Contains(expertiseArea));

        if (!string.IsNullOrWhiteSpace(schoolName))
            query = query.Where(ip => ip.Education != null && ip.Education.Contains(schoolName));

        if (!string.IsNullOrWhiteSpace(companyName))
            query = query.Where(ip => ip.WorkExperience != null && ip.WorkExperience.Contains(companyName));

        if (isDescending == false)
            query = query.OrderBy(ip => ip.CreatedAt);
        else
            query = query.OrderByDescending(ip => ip.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((Math.Max(pageNumber, 1) - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}
