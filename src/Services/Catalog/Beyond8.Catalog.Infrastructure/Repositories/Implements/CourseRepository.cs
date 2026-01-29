using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Catalog.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Repositories.Implements
{
    public class CourseRepository(CatalogDbContext context) : PostgresRepository<Course>(context), ICourseRepository
    {
        public async Task<(List<Course> Items, int TotalCount)> SearchCoursesAsync(
            int pageNumber,
            int pageSize,
            string? keyword,
            Guid? categoryId,
            Guid? instructorId,
            CourseStatus? status,
            CourseLevel? level,
            string? language,
            decimal? minPrice,
            decimal? maxPrice,
            decimal? minRating,
            int? minStudents,
            bool? isActive,
            bool? isDescending,
            bool? isRandom)
        {
            var effectiveStatus = status ?? CourseStatus.Published;

            var query = context.Courses
                .Include(c => c.Category)
                .Where(c => c.InstructorVerificationStatus == InstructorVerificationStatus.Verified)
                .Where(c => c.Status == effectiveStatus);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.Trim().ToLowerInvariant();
                query = query.Where(c =>
                    c.Title.ToLower().Contains(lowerKeyword) ||
                    c.Description.ToLower().Contains(lowerKeyword) ||
                    (c.ShortDescription != null && c.ShortDescription.ToLower().Contains(lowerKeyword)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (instructorId.HasValue)
            {
                query = query.Where(c => c.InstructorId == instructorId.Value);
            }

            if (level.HasValue)
            {
                query = query.Where(c => c.Level == level.Value);
            }

            if (!string.IsNullOrWhiteSpace(language))
            {
                query = query.Where(c => c.Language == language);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(c => c.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            if (minRating.HasValue)
            {
                query = query.Where(c => c.AvgRating >= minRating.Value);
            }

            if (minStudents.HasValue)
            {
                query = query.Where(c => c.TotalStudents >= minStudents.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(c => c.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            List<Course> items;
            if (isRandom.HasValue && isRandom.Value)
            {
                // Dùng random() trên DB thay vì load hết vào memory
                items = await query
                    .OrderBy(_ => EF.Functions.Random())
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }
            else
            {
                query = isDescending.HasValue && isDescending.Value
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt);

                items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            return (items, totalCount);
        }
    }
}
