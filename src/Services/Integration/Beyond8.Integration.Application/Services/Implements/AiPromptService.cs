using System.Text.Json;
using System.Text.RegularExpressions;
using Beyond8.Common.Caching;
using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Prompts;
using Beyond8.Integration.Application.Mappings.AiIntegrationMappings;
using Beyond8.Integration.Application.Services.Interfaces;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace Beyond8.Integration.Application.Services.Implements
{
    public class AiPromptService(
        IUnitOfWork unitOfWork,
        ILogger<AiPromptService> logger,
        ICacheService cacheService) : IAiPromptService
    {
        private static readonly Regex VersionRegex = new(@"(\d+)\.(\d+)(?:\.(\d+))?");
        private const string CacheKeyPrefix = "aiprompt";
        private static readonly TimeSpan CacheExpiryById = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan CacheExpiryList = TimeSpan.FromMinutes(2);

        private record PagedPromptsCache(List<AiPromptResponse> Items, int TotalCount);

        public async Task<ApiResponse<AiPromptResponse>> CreatePromptAsync(CreateAiPromptRequest request, Guid userId)
        {
            try
            {
                var existingPrompt = await unitOfWork.AiPromptRepository.FindOneAsync(p => p.Name == request.Name);
                if (existingPrompt != null)
                {
                    logger.LogWarning("AI prompt already exists with name: {Name}", request.Name);
                    return ApiResponse<AiPromptResponse>.FailureResponse("Tên prompt AI đã tồn tại.");
                }

                string initialVersion = string.IsNullOrWhiteSpace(request.Version) ? "1.0.0" : request.Version;

                var prompt = request.ToEntity(userId, initialVersion);

                await unitOfWork.AiPromptRepository.AddAsync(prompt);
                await unitOfWork.SaveChangesAsync();

                var response = prompt.ToResponse();
                await cacheService.SetAsync(CacheKeyById(prompt.Id), response, CacheExpiryById);

                logger.LogInformation("AI prompt created successfully with ID: {Id}", prompt.Id);
                return ApiResponse<AiPromptResponse>.SuccessResponse(response, "Tạo prompt AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating AI prompt");
                return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi tạo prompt AI.");
            }
        }

        public async Task<ApiResponse<AiPromptResponse>> GetPromptByIdAsync(Guid id)
        {
            try
            {
                var cached = await cacheService.GetAsync<AiPromptResponse>(CacheKeyById(id));
                if (cached != null)
                    return ApiResponse<AiPromptResponse>.SuccessResponse(cached, "Lấy prompt AI thành công.");

                var entity = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
                if (entity == null)
                    return ApiResponse<AiPromptResponse>.FailureResponse("Không tìm thấy prompt AI.");

                var response = entity.ToResponse();
                await cacheService.SetAsync(CacheKeyById(id), response, CacheExpiryById);
                return ApiResponse<AiPromptResponse>.SuccessResponse(response, "Lấy prompt AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting AI prompt by ID {Id}", id);
                return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi lấy prompt AI.");
            }
        }

        public async Task<ApiResponse<AiPromptResponse>> GetPromptByNameAsync(string name)
        {
            try
            {
                var entity = await unitOfWork.AiPromptRepository.GetActiveByNameAsync(name);
                if (entity == null)
                    return ApiResponse<AiPromptResponse>.FailureResponse("Không tìm thấy prompt AI.");
                return ApiResponse<AiPromptResponse>.SuccessResponse(entity.ToResponse(), "Lấy prompt AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error getting AI prompt by name {Name}", name);
                return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi lấy prompt AI.");
            }
        }

        public async Task<ApiResponse<AiPromptResponse>> UpdatePromptAsync(Guid id, UpdateAiPromptRequest request, Guid userId)
        {
            try
            {
                var currentPrompt = await unitOfWork.AiPromptRepository.GetByIdAsync(id);
                if (currentPrompt == null)
                    return ApiResponse<AiPromptResponse>.FailureResponse("Không tìm thấy prompt AI.");

                if (!IsFunctionalChange(currentPrompt, request))
                {
                    currentPrompt.ApplyMetadataOnly(request, userId);
                    await unitOfWork.AiPromptRepository.UpdateAsync(id, currentPrompt);
                    await unitOfWork.SaveChangesAsync();
                    await InvalidatePromptCacheAsync(id);

                    logger.LogInformation("Updated metadata for prompt {Name} v{Version}", currentPrompt.Name, currentPrompt.Version);
                    return ApiResponse<AiPromptResponse>.SuccessResponse(currentPrompt.ToResponse(), "Cập nhật thông tin prompt thành công.");
                }

                var nextVersion = await CalculateNextVersionAsync(currentPrompt.Name);
                var newVersionPrompt = currentPrompt.ToNewVersionEntity(request, nextVersion);

                await unitOfWork.AiPromptRepository.AddAsync(newVersionPrompt);
                currentPrompt.IsActive = false;
                await unitOfWork.AiPromptRepository.UpdateAsync(currentPrompt.Id, currentPrompt);
                await unitOfWork.SaveChangesAsync();
                await InvalidatePromptCacheAsync(id);

                logger.LogInformation("Created new version for prompt {Name}: {OldVer} -> {NewVer}",
                    currentPrompt.Name, currentPrompt.Version, newVersionPrompt.Version);
                return ApiResponse<AiPromptResponse>.SuccessResponse(newVersionPrompt.ToResponse(),
                    $"Đã cập nhật nội dung và tạo phiên bản mới: {newVersionPrompt.Version}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error updating AI prompt ID {Id}", id);
                return ApiResponse<AiPromptResponse>.FailureResponse("Đã xảy ra lỗi khi cập nhật prompt AI.");
            }
        }

        public async Task<ApiResponse<List<AiPromptResponse>>> GetAllPromptsAsync(PaginationRequest pagination)
        {
            try
            {
                var key = CacheKeyList(pagination.PageNumber, pagination.PageSize);
                var cached = await cacheService.GetAsync<PagedPromptsCache>(key);
                if (cached != null)
                    return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                        cached.Items, cached.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách prompt AI thành công.");

                var prompts = await unitOfWork.AiPromptRepository.GetPagedAsync(
                    pageNumber: pagination.PageNumber,
                    pageSize: pagination.PageSize,
                    filter: null,
                    orderBy: query => query.OrderByDescending(p => p.CreatedAt)
                );
                var responses = prompts.Items.Select(p => p.ToResponse()).ToList();
                await cacheService.SetAsync(key, new PagedPromptsCache(responses, prompts.TotalCount), CacheExpiryList);

                logger.LogInformation("Retrieved {Count} AI prompts", responses.Count);
                return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                    responses, prompts.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách prompt AI thành công.");
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
                var key = CacheKeyListByCategory(category, pagination.PageNumber, pagination.PageSize);
                var cached = await cacheService.GetAsync<PagedPromptsCache>(key);
                if (cached != null)
                    return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                        cached.Items, cached.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách prompt AI theo danh mục thành công.");

                var prompts = await unitOfWork.AiPromptRepository.GetPagedAsync(
                    pageNumber: pagination.PageNumber,
                    pageSize: pagination.PageSize,
                    filter: p => (int)p.Category == category,
                    orderBy: query => query.OrderByDescending(p => p.CreatedAt)
                );
                var responses = prompts.Items.Select(p => p.ToResponse()).ToList();
                await cacheService.SetAsync(key, new PagedPromptsCache(responses, prompts.TotalCount), CacheExpiryList);

                logger.LogInformation("Retrieved {Count} AI prompts for category {Category}", responses.Count, category);
                return ApiResponse<List<AiPromptResponse>>.SuccessPagedResponse(
                    responses, prompts.TotalCount, pagination.PageNumber, pagination.PageSize, "Lấy danh sách prompt AI theo danh mục thành công.");
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
                    return ApiResponse<bool>.FailureResponse("Không tìm thấy prompt AI.");

                prompt.IsActive = false;
                await unitOfWork.AiPromptRepository.UpdateAsync(id, prompt);

                await unitOfWork.SaveChangesAsync();
                await InvalidatePromptCacheAsync(id);

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
                    return ApiResponse<bool>.FailureResponse("Không tìm thấy prompt AI.");

                prompt.IsActive = !prompt.IsActive;
                await unitOfWork.AiPromptRepository.UpdateAsync(id, prompt);

                await unitOfWork.SaveChangesAsync();
                await InvalidatePromptCacheAsync(id);

                logger.LogInformation("AI prompt status toggled with ID: {Id}, IsActive: {Status}", id, prompt.IsActive);
                return ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái prompt AI thành công.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error toggling AI prompt status with ID {Id}", id);
                return ApiResponse<bool>.FailureResponse("Đã xảy ra lỗi khi cập nhật trạng thái prompt AI.");
            }
        }

        private static string CacheKeyById(Guid id) => $"{CacheKeyPrefix}:{id}";
        private static string CacheKeyList(int page, int size) => $"{CacheKeyPrefix}:list:p{page}:s{size}";
        private static string CacheKeyListByCategory(int category, int page, int size) => $"{CacheKeyPrefix}:list:cat{category}:p{page}:s{size}";

        private Task InvalidatePromptCacheAsync(Guid id) => cacheService.RemoveAsync(CacheKeyById(id));

        private static bool IsFunctionalChange(AiPrompt current, UpdateAiPromptRequest request) =>
            (request.Template != null && current.Template != request.Template) ||
            (request.SystemPrompt != null && current.SystemPrompt != request.SystemPrompt) ||
            (request.Temperature != null && current.Temperature != request.Temperature.Value) ||
            (request.TopP != null && current.TopP != request.TopP.Value) ||
            (request.MaxTokens != null && current.MaxTokens != request.MaxTokens.Value) ||
            (request.DefaultParameters != null && JsonSerializer.Serialize(request.DefaultParameters) != (current.DefaultParameters ?? "")) ||
            (request.Variables != null && JsonSerializer.Serialize(request.Variables) != (current.Variables ?? ""));

        private async Task<string> CalculateNextVersionAsync(string name)
        {
            var allPrompts = await unitOfWork.AiPromptRepository.GetAllAsync(p => p.Name == name);
            if (allPrompts.Count == 0) return "1.0.0";
            var allVersions = allPrompts.Select(p => p.Version).ToList();
            var maxVersion = allVersions.Max(v => ParseVersion(v));
            return $"{maxVersion!.Major}.{maxVersion.Minor + 1}.0";
        }

        private class SemVer : IComparable<SemVer>
        {
            public int Major { get; set; }
            public int Minor { get; set; }
            public int Patch { get; set; }

            public int CompareTo(SemVer? other)
            {
                if (other == null) return 1;
                if (Major != other.Major) return Major.CompareTo(other.Major);
                if (Minor != other.Minor) return Minor.CompareTo(other.Minor);
                return Patch.CompareTo(other.Patch);
            }
        }

        private SemVer ParseVersion(string versionString)
        {
            var match = VersionRegex.Match(versionString);
            if (match.Success)
            {
                return new SemVer
                {
                    Major = int.Parse(match.Groups[1].Value),
                    Minor = int.Parse(match.Groups[2].Value),
                    Patch = match.Groups[3].Success ? int.Parse(match.Groups[3].Value) : 0
                };
            }
            return new SemVer { Major = 1, Minor = 0, Patch = 0 };
        }
    }
}
