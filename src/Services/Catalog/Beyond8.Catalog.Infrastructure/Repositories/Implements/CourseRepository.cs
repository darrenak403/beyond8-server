using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Beyond8.Catalog.Application.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Repositories.Implements
{
    public class CourseRepository(CatalogDbContext context) : PostgresRepository<Course>(context), ICourseRepository
    {
        private static IQueryable<Course> ApplyFullTextSearch(IQueryable<Course> query, string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return query;

            var plainText = StringHelper.FormatSearchTermPlain(keyword);
            if (string.IsNullOrEmpty(plainText))
                return query;

            // PlainToTsQuery bên trong lambda để EF dịch sang SQL (plainto_tsquery('simple', @p))
            return query.Where(c => c.SearchVector != null &&
                c.SearchVector.Matches(EF.Functions.PlainToTsQuery("simple", plainText)));
        }

        private static IOrderedQueryable<Course> OrderBySearchRank(IQueryable<Course> query, string? keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return query.OrderByDescending(c => c.CreatedAt);

            var plainText = StringHelper.FormatSearchTermPlain(keyword);
            if (string.IsNullOrEmpty(plainText))
                return query.OrderByDescending(c => c.CreatedAt);

            return query.OrderByDescending(c =>
                c.SearchVector!.Rank(EF.Functions.PlainToTsQuery("simple", plainText)));
        }

        private static IQueryable<Course> ApplySearchFilters(
            IQueryable<Course> query,
            string? keyword,
            string? categoryName,
            string? instructorName,
            CourseStatus? status,
            CourseLevel? level,
            string? language,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating,
            bool? isActive)
        {
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(lowerKeyword) ||
                    c.Description.ToLower().Contains(lowerKeyword) ||
                    (c.ShortDescription != null && c.ShortDescription.ToLower().Contains(lowerKeyword)));
            }
            if (!string.IsNullOrWhiteSpace(categoryName))
                query = query.Where(c => c.Category.Name == categoryName || (c.Category.Parent != null && c.Category.Parent.Name == categoryName));
            if (!string.IsNullOrWhiteSpace(instructorName))
                query = query.Where(c => c.InstructorName == instructorName);
            if (status.HasValue)
                query = query.Where(c => c.Status == status.Value);
            if (level.HasValue && level.Value != CourseLevel.All)
                query = query.Where(c => c.Level == level.Value);
            if (!string.IsNullOrWhiteSpace(language))
                query = query.Where(c => c.Language == language);
            if (minPrice.HasValue)
                query = query.Where(c => c.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(c => c.Price <= maxPrice.Value);
            if (minRating.HasValue)
                query = query.Where(c => c.AvgRating >= minRating.Value);
            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);
            return query;
        }

        private static IQueryable<Course> ApplyExcludedCourseIds(IQueryable<Course> query, List<Guid>? excludedCourseIds)
        {
            if (excludedCourseIds == null || excludedCourseIds.Count == 0)
                return query;
            return query.Where(c => !excludedCourseIds.Contains(c.Id));
        }

        private static async Task<(List<Course> Items, int TotalCount)> ExecutePagedQueryAsync(
            IQueryable<Course> query,
            int pageNumber,
            int pageSize,
            bool? isDescending,
            bool? isDescendingPrice,
            bool? isDescendingRating,
            bool? isRandom)
        {
            var totalCount = await query.CountAsync();
            List<Course> items;
            if (isRandom == true)
            {
                items = await query
                    .OrderBy(_ => EF.Functions.Random())
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                query = isDescending == true
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt);
                items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            }
            if (isDescendingPrice == true)
                items = items.OrderByDescending(c => c.Price).ToList();
            else if (isDescendingPrice == false)
                items = items.OrderBy(c => c.Price).ToList();
            if (isDescendingRating == true)
                items = items.OrderByDescending(c => c.AvgRating ?? 0).ToList();
            else if (isDescendingRating == false)
                items = items.OrderBy(c => c.AvgRating ?? 0).ToList();
            return (items, totalCount);
        }

        public async Task<(List<Course> Items, int TotalCount)> SearchCoursesAsync(
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
            bool? isDescendingRating,
            bool? isRandom,
            List<Guid>? excludedCourseIds = null)
        {
            var query = context.Courses
                .Include(c => c.Category).ThenInclude(cat => cat.Parent)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Video)
                .AsQueryable();

            query = ApplySearchFilters(query, keyword, categoryName, instructorName, status, level, language, minPrice, maxPrice, minRating, isActive);
            query = ApplyExcludedCourseIds(query, excludedCourseIds);

            return await ExecutePagedQueryAsync(query, pageNumber, pageSize, isDescending, isDescendingPrice, isDescendingRating, isRandom);
        }

        public async Task<(List<Course> Items, int TotalCount)> SearchCoursesInstructorAsync(int pageNumber,
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
            bool? isDescendingRating,
            bool? isRandom,
            Guid? instructorId = null)
        {
            var query = context.Courses
                .Include(c => c.Category).ThenInclude(cat => cat.Parent)
                .Include(c => c.Documents)
                .AsQueryable();

            if (instructorId.HasValue)
                query = query.Where(c => c.InstructorId == instructorId.Value);
            else
            {
                query = query.Where(c => c.InstructorVerificationStatus == InstructorVerificationStatus.Verified);
                query = query.Where(c => c.Status == (status ?? CourseStatus.Published));
            }

            query = ApplySearchFilters(query, keyword, categoryName, instructorName, status, level, language, minPrice, maxPrice, minRating, isActive);
            return await ExecutePagedQueryAsync(query, pageNumber, pageSize, isDescending, isDescendingPrice, isDescendingRating, isRandom);
        }

        public async Task<(List<Course> Items, int TotalCount)> SearchCoursesAdminAsync(int pageNumber,
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
            bool? isDescendingRating,
            bool? isRandom)
        {
            var query = context.Courses
                .Include(c => c.Category).ThenInclude(cat => cat.Parent)
                .Where(c => c.Status != CourseStatus.Draft)
                .AsQueryable();

            query = ApplySearchFilters(query, keyword, categoryName, instructorName, status, level, language, minPrice, maxPrice, minRating, isActive);
            return await ExecutePagedQueryAsync(query, pageNumber, pageSize, isDescending, isDescendingPrice, isDescendingRating, isRandom);
        }

        public async Task<(List<Course> Items, int TotalCount)> GetMostPopularCoursesAsync(int pageNumber, int pageSize)
        {
            var query = context.Courses
                .Include(c => c.Category).ThenInclude(cat => cat.Parent)
                .Include(c => c.Documents)
                .Include(c => c.Sections).ThenInclude(s => s.Lessons).ThenInclude(l => l.Video)
                .Where(c => c.IsActive && c.Status == CourseStatus.Published)
                .OrderByDescending(c => c.TotalStudents)
                .ThenByDescending(c => c.AvgRating ?? 0)
                .AsQueryable();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<Course> Items, int TotalCount)> FullTextSearchCoursesAsync(
            int pageNumber,
            int pageSize,
            string keyword,
            List<Guid>? excludedCourseIds = null)
        {
            var query = context.Courses
                .Include(c => c.Category).ThenInclude(cat => cat.Parent)
                .Include(c => c.Documents)
                .Where(c => c.IsActive && c.Status == CourseStatus.Published)
                .AsQueryable();

            // If keyword is provided, apply full-text search
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = ApplyFullTextSearch(query, keyword);
            }

            if (excludedCourseIds != null && excludedCourseIds.Any())
            {
                query = query.Where(c => !excludedCourseIds.Contains(c.Id));
            }

            var totalCount = await query.CountAsync();

            // Order by search relevance rank if keyword exists, otherwise by creation date
            IOrderedQueryable<Course> orderedQuery = string.IsNullOrWhiteSpace(keyword)
                ? query.OrderByDescending(c => c.CreatedAt)
                : OrderBySearchRank(query, keyword);

            var items = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
