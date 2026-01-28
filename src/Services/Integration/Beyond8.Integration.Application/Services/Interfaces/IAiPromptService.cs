using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Prompts;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface IAiPromptService
    {
        Task<ApiResponse<AiPromptResponse>> CreatePromptAsync(CreateAiPromptRequest request, Guid userId);
        Task<ApiResponse<AiPromptResponse>> UpdatePromptAsync(Guid id, UpdateAiPromptRequest request, Guid userId);
        Task<ApiResponse<AiPromptResponse>> GetPromptByIdAsync(Guid id);
        Task<ApiResponse<AiPromptResponse>> GetPromptByNameAsync(string name);
        Task<ApiResponse<List<AiPromptResponse>>> GetAllPromptsAsync(PaginationRequest pagination);
        Task<ApiResponse<bool>> DeletePromptAsync(Guid id);
        Task<ApiResponse<bool>> TogglePromptStatusAsync(Guid id);
    }
}
