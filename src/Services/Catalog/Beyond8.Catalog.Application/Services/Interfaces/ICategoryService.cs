using System;
using Beyond8.Common.Utilities;
using Beyond8.Catalog.Application.Dtos.Categories;

namespace Beyond8.Catalog.Application.Services.Interfaces;


public interface ICategoryService
{
    Task<ApiResponse<CategorySimpleResponse>> CreateCategoryAsync(CreateCategoryRequest request);
    // Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request);
    // Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(Guid id);
    // Task<ApiResponse<List<CategoryResponse>>> GetAllCategoriesAsync();
    // Task<ApiResponse<List<CategoryResponse>>> GetCategoriesByParentIdAsync(Guid parentId);
    // Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id);
    // Task<ApiResponse<bool>> ToggleCategoryStatusAsync(Guid id);
    Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync();
}
