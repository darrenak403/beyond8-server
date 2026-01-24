using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IInstructorProfileRepository : IGenericRepository<InstructorProfile>
    {
        /// <summary>
        /// Tìm kiếm giảng viên (query phức tạp với Include, jsonb)
        /// </summary>
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

        /// <summary>
        /// Lấy danh sách giảng viên đã phê duyệt (có Include User)
        /// </summary>
        Task<(List<InstructorProfile> Profiles, int TotalCount)> GetVerifiedInstructorsAsync(
            int pageNumber,
            int pageSize);

        /// <summary>
        /// Lấy top giảng viên theo rating (có Include User)
        /// </summary>
        Task<List<InstructorProfile>> GetTopInstructorsByRatingAsync(int count);
    }
}