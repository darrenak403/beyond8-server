using System;
using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Common.Utilities;

namespace Beyond8.Catalog.Application.Mappings.CategoyMappings;

public static class CategoryMappings
{
    public static Category ToEntity(this CreateCategoryRequest request, Category? parentCategory)
    {
        var slug = request.Name.ToSlug();
        var level = parentCategory != null ? parentCategory.Level + 1 : 0;
        string? path = null;
        if (parentCategory != null)
        {
            path = string.IsNullOrEmpty(parentCategory.Path)
                ? parentCategory.Id.ToString()
                : $"{parentCategory.Path}/{parentCategory.Id}";
        }
        return new Category
        {
            Name = request.Name,
            Slug = slug,
            Description = request.Description,
            ParentId = request.ParentId,
            Level = level,
            Path = path,
            IsActive = true,
        };
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
            Description = entity.Description,
            ParentId = entity.ParentId
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
            SubCategories = entity.SubCategories?
                .Select(c => c.ToCategoryTreeDto())
                .OrderBy(c => c.Level).ThenBy(c => c.Name)
                .ToList() ?? []
        };
    }
}
