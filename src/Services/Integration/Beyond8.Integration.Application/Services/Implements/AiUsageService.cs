using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Application.Mappings.AiIntegrationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Repositories.Interfaces;

namespace Beyond8.Integration.Application.Services.Implements;

public class AiUsageService(
    IUnitOfWork unitOfWork,
    ILogger<AiUsageService> logger) : IAiUsageService
{
    public async Task<ApiResponse<AiUsageResponse>> TrackUsageAsync(AiUsageRequest request)
    {
        try
        {
            var aiUsage = request.ToEntity();
            aiUsage.TotalTokens = aiUsage.InputTokens + aiUsage.OutputTokens;
            aiUsage.TotalCost = aiUsage.InputCost + aiUsage.OutputCost;

            await unitOfWork.AiUsageRepository.AddAsync(aiUsage);
            await unitOfWork.SaveChangesAsync();

            logger.LogInformation("AI usage tracked successfully for user {UserId} with provider {Provider}", request.UserId, request.Provider);

            return ApiResponse<AiUsageResponse>.SuccessResponse(aiUsage.ToResponse(), "Ghi nhận sử dụng AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error tracking AI usage for user {UserId}", request.UserId);
            return ApiResponse<AiUsageResponse>.FailureResponse("Đã xảy ra lỗi khi ghi nhận sử dụng AI.");
        }
    }

    public async Task<ApiResponse<AiUsageResponse>> GetUsageByIdAsync(Guid id)
    {
        try
        {
            var usage = await unitOfWork.AiUsageRepository.GetByIdAsync(id);
            if (usage == null)
            {
                logger.LogWarning("AI usage not found with ID: {Id}", id);
                return ApiResponse<AiUsageResponse>.FailureResponse("Không tìm thấy thông tin sử dụng AI.");
            }

            return ApiResponse<AiUsageResponse>.SuccessResponse(usage.ToResponse(), "Lấy thông tin sử dụng AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting AI usage by ID {Id}", id);
            return ApiResponse<AiUsageResponse>.FailureResponse("Đã xảy ra lỗi khi lấy thông tin sử dụng AI.");
        }
    }

    public async Task<ApiResponse<List<AiUsageResponse>>> GetUserUsageHistoryAsync(Guid userId, PaginationRequest pagination)
    {
        try
        {
            var usages = await unitOfWork.AiUsageRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: u => u.UserId == userId,
                orderBy: query => query.OrderByDescending(u => u.CreatedAt)
            );

            var responses = usages.Items.Select(u => u.ToResponse()).ToList();

            logger.LogInformation("Retrieved {Count} AI usages for user {UserId}", responses.Count, userId);

            return ApiResponse<List<AiUsageResponse>>.SuccessPagedResponse(
                responses,
                usages.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy lịch sử sử dụng AI thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting usage history for user {UserId}", userId);
            return ApiResponse<List<AiUsageResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy lịch sử sử dụng AI.");
        }
    }

    public async Task<ApiResponse<List<AiUsageResponse>>> GetUsageByProviderAsync(Guid userId, int provider, PaginationRequest pagination)
    {
        try
        {
            var usages = await unitOfWork.AiUsageRepository.GetPagedAsync(
                pageNumber: pagination.PageNumber,
                pageSize: pagination.PageSize,
                filter: u => u.UserId == userId && (int)u.Provider == provider,
                orderBy: query => query.OrderByDescending(u => u.CreatedAt)
            );

            var responses = usages.Items.Select(u => u.ToResponse()).ToList();

            logger.LogInformation("Retrieved {Count} AI usages for user {UserId} with provider {Provider}", responses.Count, userId, provider);

            return ApiResponse<List<AiUsageResponse>>.SuccessPagedResponse(
                responses,
                usages.TotalCount,
                pagination.PageNumber,
                pagination.PageSize,
                "Lấy lịch sử sử dụng AI theo nhà cung cấp thành công.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting usage by provider for user {UserId}", userId);
            return ApiResponse<List<AiUsageResponse>>.FailureResponse("Đã xảy ra lỗi khi lấy lịch sử sử dụng AI theo nhà cung cấp.");
        }
    }
}
