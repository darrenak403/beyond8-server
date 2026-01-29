using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Usages;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IAiUsageService
    {
        Task<ApiResponse<AiUsageResponse>> TrackUsageAsync(AiUsageRequest request);
        Task<ApiResponse<AiUsageResponse>> GetUsageByIdAsync(Guid id);
        Task<ApiResponse<List<AiUsageResponse>>> GetUserUsageHistoryAsync(Guid userId, PaginationRequest pagination);
        Task<ApiResponse<List<AiUsageResponse>>> GetUsageByProviderAsync(Guid userId, int provider, PaginationRequest pagination);
        Task<ApiResponse<List<AiUsageResponse>>> GetAllUsageAsync(AiUsageSearchRequest searchRequest);
        Task<ApiResponse<AiUsageStatisticsResponse>> GetUsageStatisticsAsync();
    }
}
