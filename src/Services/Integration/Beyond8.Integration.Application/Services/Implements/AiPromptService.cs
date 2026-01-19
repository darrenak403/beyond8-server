using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Mappings.AiIntegrationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements;

public class AiPromptService(
    IUnitOfWork unitOfWork,
    ILogger<AiPromptService> logger) : IAiPromptService
{
    public async Task<ApiResponse<AiPromptResponse>> CreatePromptAsync(CreateAiPromptRequest request, Guid userId)
    {
        try
        {
            var prompt = request.ToEntity(userId);

            await unitOfWork.AiPromptRepository.AddAsync(prompt);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("AI prompt created successfully with ID: {Id}", prompt.Id);

            return ApiResponse<AiPromptResponse>.SuccessResponse(prompt.ToResponse(), "Tạo prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating AI prompt");
            return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi tạo prompt AI.");
        }
    }

    public async Task<ApiResponse<AiPromptResponse>> UpdatePromptAsync(Guid id, UpdateAiPromptRequest request, Guid userId)
    {
        try
        {
            var prompt = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
            if (prompt == null)
            {
                logger.LogWarning("AI prompt not found with ID: {Id}", id);
                return ApiResponse<AiPromptResponse>.FailureResponse("Không tìm thấy prompt AI.");
            }

            prompt.UpdateFromRequest(request, userId);

            await unitOfWork.AiPromptRepository.UpdateAsync(id, prompt);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("AI prompt updated successfully with ID: {Id}", id);

            return ApiResponse<AiPromptResponse>.SuccessResponse(prompt.ToResponse(), "Cập nhật prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating AI prompt with ID {Id}", id);
            return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật prompt AI.");
        }
    }

    public async Task<ApiResponse<AiPromptResponse>> GetPromptByIdAsync(Guid id)
    {
        try
        {
            var prompt = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
            if (prompt == null)
            {
                logger.LogWarning("AI prompt not found with ID: {Id}", id);
                return ApiResponse<AiPromptResponse>.FailureResponse("Không tìm thấy prompt AI.");
            }

            return ApiResponse<AiPromptResponse>.SuccessResponse(prompt.ToResponse(), "Lấy thông tin prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting AI prompt by ID {Id}", id);
            return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin prompt AI.");
        }
    }

    public async Task<ApiResponse<List<AiPromptResponse>>> GetAllPromptsAsync(PaginationRequest pagination)
    {
        try
        {
            var prompts = await unitOfWork.AiPromptRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: null,
                orderBy: query => query.OrderByDescending(p => p.CreatedAt)
            );

            var responses = prompts.Items.Select(p => p.ToResponse()).ToList();

            logger.LogInformation("Retrieved {Count} AI prompts", responses.Count);

            return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                responses,
                prompts.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all AI prompts");
            return ApiResponse<List<AiPromptResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách prompt AI.");
        }
    }

    public async Task<ApiResponse<List<AiPromptResponse>>> GetPromptsByCategoryAsync(int category, PaginationRequest pagination)
    {
        try
        {
            var prompts = await unitOfWork.AiPromptRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: p => (int)p.Category == category,
                orderBy: query => query.OrderByDescending(p => p.CreatedAt)
            );

            var responses = prompts.Items.Select(p => p.ToResponse()).ToList();

            logger.LogInformation("Retrieved {Count} AI prompts for category {Category}", responses.Count, category);

            return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                responses,
                prompts.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy danh sách prompt AI theo danh mục thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting AI prompts by category {Category}", category);
            return ApiResponse<List<AiPromptResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy danh sách prompt AI theo danh mục.");
        }
    }

    public async Task<ApiResponse<bool>> DeletePromptAsync(Guid id)
    {
        try
        {
            var prompt = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
            if (prompt == null)
            {
                logger.LogWarning("AI prompt not found with ID: {Id}", id);
                return ApiResponse<bool>.FailureResponse("Không tìm thấy prompt AI.");
            }

            await unitOfWork.AiPromptRepository.DeleteAsync(id);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("AI prompt deleted successfully with ID: {Id}", id);

            return ApiResponse<bool>.SuccessResponse(true, "Xóa prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting AI prompt with ID {Id}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi xóa prompt AI.");
        }
    }

    public async Task<ApiResponse<bool>> TogglePromptStatusAsync(Guid id)
    {
        try
        {
            var prompt = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
            if (prompt == null)
            {
                logger.LogWarning("AI prompt not found with ID: {Id}", id);
                return ApiResponse<bool>.FailureResponse("Không tìm thấy prompt AI.");
            }

            prompt.IsActive = !prompt.IsActive;

            await unitOfWork.AiPromptRepository.UpdateAsync(id, prompt);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("AI prompt status toggled successfully with ID: {Id}, new status: {Status}", id, prompt.IsActive);

            return ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái prompt AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error toggling AI prompt status with ID {Id}", id);
            return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật trạng thái prompt AI.");
        }
    }
}
