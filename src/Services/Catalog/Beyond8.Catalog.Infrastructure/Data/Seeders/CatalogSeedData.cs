using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Enums; // Đảm bảo namespace này tồn tại chứa Enum CategoryType
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Catalog.Infrastructure.Data.Seeders
{
    public static class CatalogSeedData
    {
        public static async Task SeedCategoriesAsync(CatalogDbContext context)
        {
            // Seed data đã bị tắt để test bằng API
            // if (await context.Categories.AnyAsync())
            // {
            //     return;
            // }

            // var categories = new List<Category>();
            // ... rest of seed data commented out
        }

        public static async Task SeedCoursesAsync(CatalogDbContext context)
        {
            // Seed data đã bị tắt để test bằng API
            // if (await context.Courses.AnyAsync())
            // {
            //     return;
            // }

            // ... rest of seed data commented out
        }

        private static Category CreateCategory(
            string name,
            string slug,
            string description,
            CategoryType type = CategoryType.Other)
        {
            var id = Guid.NewGuid();
            return new Category
            {
                Id = id,
                Name = name,
                Slug = slug,
                Description = description,
                ParentId = null,
                Level = 0,
                Path = id.ToString(),
                IsActive = true,
                TotalCourses = 0,
                IsRoot = true,
                Type = type
            };
        }

        private static IEnumerable<Category> CreateSubCategories(
            Category parent,
            (string Name, string Slug, string Desc)[] subs)
        {
            var list = new List<Category>();
            foreach (var sub in subs)
            {
                var id = Guid.NewGuid();
                list.Add(new Category
                {
                    Id = id,
                    Name = sub.Name,
                    Slug = sub.Slug,
                    Description = sub.Desc,
                    ParentId = parent.Id,
                    Level = parent.Level + 1,
                    Path = $"{parent.Path}/{id}",
                    IsActive = true,
                    TotalCourses = 0,
                    IsRoot = false,
                    Type = parent.Type
                });
            }
            return list;
        }
    }
}