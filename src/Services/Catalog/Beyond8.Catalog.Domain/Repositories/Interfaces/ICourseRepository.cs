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
            string? keyword = null,
            string? categoryName = null,
            string? instructorName = null,
            CourseStatus? status = null,
            CourseLevel? level = null,
            string? language = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null,
            int? minStudents = null,
            bool? isActive = null,
            bool? isDescending = null,
            bool? isDescendingPrice = null,
            bool? isDescendingRating = null,
            bool? isRandom = null,
            List<Guid>? excludedCourseIds = null);

        Task<(List<Course> Items, int TotalCount)> SearchCoursesInstructorAsync(
            int pageNumber,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            string? instructorName = null,
            CourseStatus? status = null,
            CourseLevel? level = null,
            string? language = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null,
            int? minStudents = null,
            bool? isActive = null,
            bool? isDescending = null,
            bool? isDescendingPrice = null,
            bool? isDescendingRating = null,
            bool? isRandom = null,
            Guid? instructorId = null);

        Task<(List<Course> Items, int TotalCount)> SearchCoursesAdminAsync(
            int pageNumber,
            int pageSize,
            string? keyword = null,
            string? categoryName = null,
            string? instructorName = null,
            CourseStatus? status = null,
            CourseLevel? level = null,
            string? language = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null,
            int? minStudents = null,
            bool? isActive = null,
            bool? isDescending = null,
            bool? isDescendingPrice = null,
            bool? isDescendingRating = null,
            bool? isRandom = null); 

        Task<(List<Course> Items, int TotalCount)> FullTextSearchCoursesAsync(
            int pageNumber,
            int pageSize,
            string keyword,
            List<Guid>? excludedCourseIds = null);

        Task<(List<Course> Items, int TotalCount)> GetMostPopularCoursesAsync(int pageNumber, int pageSize);
    }
}
