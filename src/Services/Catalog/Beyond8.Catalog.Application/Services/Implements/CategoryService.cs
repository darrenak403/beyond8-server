using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Mappings.CategoyMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements;

public class CategoryService(ILogger<CategoryService> logger, IUnitOfWork unitOfWork) : ICategoryService
{
    public async Task<ApiResponse<CategorySimpleResponse>> CreateCategoryAsync(CreateCategoryRequest request)
    {
        try
        {
            var existingCategory = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Name == request.Name);
            if (existingCategory != null)
            {
                logger.LogWarning("Category already exists with name: {Name}", request.Name);
                return ApiResponse<CategorySimpleResponse>.FailureResponse("Danh mục đã tồn tại.");
            }

            Category? parentCategory = null;

            if (request.ParentId.HasValue)
            {
                parentCategory = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.ParentId);

                if (parentCategory == null)
                {
                    return ApiResponse<CategorySimpleResponse>.FailureResponse("Danh mục cha không tồn tại.");
                }

                if (parentCategory.Level >= 1)
                {
                    return ApiResponse<CategorySimpleResponse>.FailureResponse("Hệ thống chỉ hỗ trợ tối đa 2 cấp danh mục cấp 1 và cấp 2.");
                }
            }

            var newCategory = request.ToEntity(parentCategory);
            await unitOfWork.CategoryRepository.AddAsync(newCategory);
            await unitOfWork.SaveChangesAsync();
            return ApiResponse<CategorySimpleResponse>.SuccessResponse(newCategory.ToSimpleResponse(), "Tạo danh mục thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating category");
            return ApiResponse<CategorySimpleResponse>.FailureResponse("Đã xảy ra lỗi khi tạo danh mục.");
        }
    }

    public async Task<ApiResponse<CategoryResponse>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
    {
        try
        {
            var category = await unitOfWork.CategoryRepository
                .AsQueryable()
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                logger.LogWarning("Category not found with id: {CategoryId}", id);
                return ApiResponse<CategoryResponse>.FailureResponse("Danh mục không tồn tại.");
            }

            // Check if new name already exists (excluding current category)
            var existingWithName = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Name == request.Name && c.Id != id);
            if (existingWithName != null)
            {
                logger.LogWarning("Category with name already exists: {Name}", request.Name);
                return ApiResponse<CategoryResponse>.FailureResponse("Danh mục với tên này đã tồn tại.");
            }

            Category? newParentCategory = null;
            if (request.ParentId.HasValue && request.ParentId != category.ParentId)
            {
                newParentCategory = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == request.ParentId);
                if (newParentCategory == null)
                {
                    return ApiResponse<CategoryResponse>.FailureResponse("Danh mục cha không tồn tại.");
                }

                // Prevent self-parent assignment
                if (newParentCategory.Id == id)
                {
                    return ApiResponse<CategoryResponse>.FailureResponse("Danh mục không thể tự trỏ tới chính nó.");
                }

                // Check level constraints
                if (newParentCategory.Level >= 1)
                {
                    return ApiResponse<CategoryResponse>.FailureResponse("Hệ thống chỉ hỗ trợ tối đa 2 cấp danh mục cấp 1 và cấp 2.");
                }
            }
            else if (!request.ParentId.HasValue && request.ParentId != category.ParentId)
            {
                newParentCategory = null;
            }

            category.UpdateFromRequest(request, newParentCategory);
            await unitOfWork.CategoryRepository.UpdateAsync(id, category);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Category updated successfully: {CategoryId}", id);
            return ApiResponse<CategoryResponse>.SuccessResponse(category.ToResponse(), "Cập nhật danh mục thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating category with id: {CategoryId}", id);
            return ApiResponse<CategoryResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật danh mục.");
        }
    }

    public async Task<ApiResponse<CategoryResponse>> GetCategoryByIdAsync(Guid id)
    {
        try
        {
            var category = await unitOfWork.CategoryRepository
                .AsQueryable()
                .Include(c => c.Parent)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category == null)
            {
                logger.LogWarning("Category not found with id: {CategoryId}", id);
                return ApiResponse<CategoryResponse>.FailureResponse("Danh mục không tồn tại.");
            }

            return ApiResponse<CategoryResponse>.SuccessResponse(category.ToResponse(), "Lấy danh mục thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting category with id: {CategoryId}", id);
            return ApiResponse<CategoryResponse>.FailureResponse("Đã xảy ra lỗi khi lấy danh mục.");
        }
    }

    public async Task<ApiResponse<List<CategoryResponse>>> GetAllCategoriesAsync(PaginationRequest pagination)
    {
        try
        {
            var paginatedCategories = await unitOfWork.CategoryRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: x => x.IsActive,
                orderBy: query => query.OrderBy(x => x.Level).ThenBy(x => x.Name)
            );

            var categoryResponses = paginatedCategories.Items
                .Select(c => c.ToResponse())
                .ToList();

            return ApiResponse<List<CategoryResponse>>.SuccessPagedResponse(
                categoryResponses,
                paginatedCategories.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách danh mục thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all categories");
            return ApiResponse<List<CategoryResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách danh mục.");
        }
    }

    public async Task<ApiResponse<List<CategoryResponse>>> GetCategoriesByParentIdAsync(Guid parentId)
    {
        try
        {
            var parentCategory = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == parentId);
            if (parentCategory == null)
            {
                logger.LogWarning("Parent category not found with id: {ParentId}", parentId);
                return ApiResponse<List<CategoryResponse>>.FailureResponse("Danh mục cha không tồn tại.");
            }

            var childCategories = await unitOfWork.CategoryRepository
                .AsQueryable()
                .Where(x => x.ParentId == parentId && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            var categoryResponses = childCategories
                .Select(c => c.ToResponse())
                .ToList();

            return ApiResponse<List<CategoryResponse>>.SuccessResponse(
                categoryResponses,
                "Lấy danh sách danh mục con thành công."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting categories by parent id: {ParentId}", parentId);
            return ApiResponse<List<CategoryResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách danh mục con.");
        }
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(Guid id)
    {
        try
        {
            var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == id);
            if (category == null)
            {
                logger.LogWarning("Category not found with id: {CategoryId}", id);
                return ApiResponse<bool>.FailureResponse("Danh mục không tồn tại.");
            }

            // Check if category has subcategories
            var hasSubcategories = await unitOfWork.CategoryRepository
                .AsQueryable()
                .AnyAsync(c => c.ParentId == id);

            if (hasSubcategories)
            {
                return ApiResponse<bool>.FailureResponse("Không thể xóa danh mục có chứa danh mục con.");
            }

            // Check if category has courses
            var hasCourses = category.Courses.Count > 0;
            if (hasCourses)
            {
                return ApiResponse<bool>.FailureResponse("Không thể xóa danh mục có chứa khóa học.");
            }

            await unitOfWork.CategoryRepository.DeleteAsync(id);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Category deleted successfully: {CategoryId}", id);
            return ApiResponse<bool>.SuccessResponse(true, "Xóa danh mục thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting category with id: {CategoryId}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa danh mục.");
        }
    }

    public async Task<ApiResponse<bool>> ToggleCategoryStatusAsync(Guid id)
    {
        try
        {
            var category = await unitOfWork.CategoryRepository.FindOneAsync(c => c.Id == id);
            if (category == null)
            {
                logger.LogWarning("Category not found with id: {CategoryId}", id);
                return ApiResponse<bool>.FailureResponse("Danh mục không tồn tại.");
            }

            category.IsActive = !category.IsActive;
            await unitOfWork.CategoryRepository.UpdateAsync(id, category);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("Category status toggled for id: {CategoryId}, new status: {IsActive}", id, category.IsActive);
            return ApiResponse<bool>.SuccessResponse(true, $"Danh mục đã được {(category.IsActive ? "kích hoạt" : "vô hiệu hóa")}.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling category status with id: {CategoryId}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi thay đổi trạng thái danh mục.");
        }
    }

    public async Task<ApiResponse<List<CategoryTreeDto>>> GetCategoryTreeAsync()
    {
        try
        {
            var allEntities = await unitOfWork.CategoryRepository
                        .AsQueryable()
                        .Where(x => x.IsActive)
                        .OrderBy(x => x.Level)
                        .ToListAsync();

            if (allEntities.Count == 0)
                return ApiResponse<List<CategoryTreeDto>>.SuccessResponse([], "Không có danh mục nào.");

            var lookup = allEntities.ToLookup(x => x.ParentId);

            foreach (var cat in allEntities)
            {
                cat.SubCategories = [.. lookup[cat.Id]];
            }

            var rootNodes = allEntities
                .Where(x => x.ParentId == null)
                .Select(x => x.ToCategoryTreeDto())
                .ToList();

            return ApiResponse<List<CategoryTreeDto>>.SuccessResponse(rootNodes, "Lấy danh sách thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting category tree");
            return ApiResponse<List<CategoryTreeDto>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách danh mục.");
        }
    }
}
