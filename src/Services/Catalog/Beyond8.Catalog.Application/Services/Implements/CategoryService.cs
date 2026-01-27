using Beyond8.Catalog.Application.Dtos.Categories;
using Beyond8.Catalog.Application.Mappings.CategoyMappings;
using Beyond8.Catalog.Application.Services.Interfaces;
using Beyond8.Catalog.Domain.Entities;
using Beyond8.Catalog.Domain.Repositories.Interfaces;
using Beyond8.Common.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Services.Implements
{
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
}

