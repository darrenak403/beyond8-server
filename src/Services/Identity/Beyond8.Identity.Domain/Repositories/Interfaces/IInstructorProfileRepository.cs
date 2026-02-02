using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IInstructorProfileRepository : IGenericRepository<InstructorProfile>
    {
        Task<(List<InstructorProfile> Items, int TotalCount)> SearchInstructorsPagedAsync(
            int pageNumber,
            int pageSize,
            string? email,
            string? fullName,
            string? phoneNumber,
            string? bio,
            string? headLine,
            string? expertiseArea,
            string? schoolName,
            string? companyName,
            VerificationStatus? verificationStatus,
            bool? isDescending);
    }
}