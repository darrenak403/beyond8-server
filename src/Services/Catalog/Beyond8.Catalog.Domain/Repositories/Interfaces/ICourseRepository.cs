using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Common.Data.Interfaces;

namespace Beyond8.Catalog.Domain.Repositories.Interfaces
{
    public interface ICourseRepository : IGenericRepository<Course>
    {
        Task<(List<Course> Items, int TotalCount)> SearchCoursesAsync(
            int pageNumber,
            int pageSize,
            string? keyword,
            string? categoryName,
            string? instructorName,
            CourseStatus? status,
            CourseLevel? level,
            string? language,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating,
            int? minStudents,
            bool? isActive,
            bool? isDescending,
            bool? isDescendingPrice,
            bool? isRandom);

        Task<(List<Course> Items, int TotalCount)> SearchCoursesInstructorAsync(
            int pageNumber,
            int pageSize,
            string? keyword,
            string? categoryName,
            string? instructorName,
            CourseStatus? status,
            CourseLevel? level,
            string? language,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating,
            int? minStudents,
            bool? isActive,
            bool? isDescending,
            bool? isDescendingPrice,
            bool? isRandom,
            Guid? instructorId = null);

        Task<(List<Course> Items, int TotalCount)> SearchCoursesAdminAsync(
                int pageNumber,
                int pageSize,
                string? keyword,
                string? categoryName,
                string? instructorName,
                CourseStatus? status,
                CourseLevel? level,
                string? language,
                decimal? minPrice,
                decimal? maxPrice,
                decimal? minRating,
                int? minStudents,
                bool? isActive,
                bool? isDescending,
                bool? isDescendingPrice,
                bool? isRandom);
    }
}
