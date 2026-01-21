using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IInstructorProfileRepository : IGenericRepository<InstructorProfile>
    {
        /// <summary>
        /// Tìm kiếm giảng viên (query phức tạp với Include, jsonb)
        /// </summary>
        Task<(List<InstructorProfile> Profiles, int TotalCount)> SearchInstructorsAsync(
            string? searchTerm,
            List<string>? expertiseAreas,
            int pageNumber,
            int pageSize);

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