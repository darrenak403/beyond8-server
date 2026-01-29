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
            var query = ((CatalogDbContext)_context).Courses.AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(c =>
                    c.Title.Contains(keyword) ||
                    c.Description.Contains(keyword) ||
                    (c.ShortDescription != null && c.ShortDescription.Contains(keyword)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(c => c.CategoryId == categoryId.Value);
            }

            if (instructorId.HasValue)
            {
                query = query.Where(c => c.InstructorId == instructorId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(c => c.Status == status.Value);
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
                var allItems = await query.ToListAsync();
                var random = new Random();
                allItems = allItems.OrderBy(x => random.Next()).ToList();
                // Apply pagination on shuffled list
                items = allItems.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            }
            else
            {
                query = isDescending.HasValue && isDescending.Value
                    ? query.OrderByDescending(c => c.CreatedAt)
                    : query.OrderBy(c => c.CreatedAt);

                // Apply pagination
                items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
            }

            return (items, totalCount);
        }
    }
}
