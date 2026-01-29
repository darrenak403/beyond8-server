using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Mappings.CategoyMappings
{
    public static class CategoryMappings
    {
        public static Category ToEntity(this CreateCategoryRequest request, Category? parentCategory)
        {
            var slug = request.Name.ToSlug();
            var level = parentCategory != null ? parentCategory.Level + 1 : 0;
            var isRoot = parentCategory == null;
            var type = parentCategory != null ? parentCategory.Type : request.Type;

            var category = new Category
            {
                Name = request.Name,
                Slug = slug,
                Description = request.Description,
                ParentId = request.ParentId,
                Level = level,
                IsActive = true,
                Type = type,
                IsRoot = isRoot
            };

            if (parentCategory != null)
            {
                category.Path = string.IsNullOrEmpty(parentCategory.Path)
                    ? parentCategory.Id.ToString()
                    : $"{parentCategory.Path}/{parentCategory.Id}";
            }
            else
            {
                category.Path = category.Id.ToString();
            }

            return category;
        }

        public static CategoryResponse ToResponse(this Category entity)
        {
            return new CategoryResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Description = entity.Description,
                ParentId = entity.ParentId,
                ParentName = entity.Parent?.Name,
                Level = entity.Level,
                Path = entity.Path,
                IsActive = entity.IsActive,
                TotalCourses = entity.TotalCourses,
                Type = entity.Type,
                IsRoot = entity.IsRoot,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }

        public static CategorySimpleResponse ToSimpleResponse(this Category entity)
        {
            return new CategorySimpleResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                ParentId = entity.ParentId,
                Type = entity.Type,
                IsRoot = entity.IsRoot
            };
        }

        public static CategoryTreeDto ToCategoryTreeDto(this Category entity)
        {
            return new CategoryTreeDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Slug = entity.Slug,
                Level = entity.Level,
                Type = entity.Type,
                IsRoot = entity.IsRoot,
                SubCategories = entity.SubCategories?
                    .Select(c => c.ToCategoryTreeDto())
                    .OrderBy(c => c.Level).ThenBy(c => c.Name)
                    .ToList() ?? []
            };
        }

        public static void UpdateFromRequest(this Category entity, UpdateCategoryRequest request)
        {
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.Slug = request.Name.ToSlug();
        }
    }
}
