using System.Text.Json;
using Beyond8.Integration.Application.Dtos.Prompts;
using Beyond8.Integration.Domain.Entities;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings
{
    public static class AiPromptMappings
    {
        public static AiPrompt ToEntity(this CreateAiPromptRequest request, Guid userId, string initialVersion)
        {
            return new AiPrompt
            {
                Name = request.Name,
                Description = request.Description,
                Category = request.Category,
                Template = request.Template,
                Version = request.Version ?? initialVersion,
                IsActive = true,
                Variables = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
                DefaultParameters = request.DefaultParameters != null ? JsonSerializer.Serialize(request.DefaultParameters) : null,
                SystemPrompt = request.SystemPrompt,
                MaxTokens = request.MaxTokens,
                Temperature = request.Temperature,
                TopP = request.TopP,
                Tags = request.Tags != null ? string.Join(", ", request.Tags) : null
            };
        }

        public static void UpdateFromRequest(this AiPrompt entity, UpdateAiPromptRequest request, Guid userId)
        {
            if (request.Description != null) entity.Description = request.Description;
            if (request.Category != null) entity.Category = request.Category.Value;
            if (request.Template != null) entity.Template = request.Template;
            if (request.Variables != null) entity.Variables = JsonSerializer.Serialize(request.Variables);
            if (request.DefaultParameters != null) entity.DefaultParameters = JsonSerializer.Serialize(request.DefaultParameters);
            if (request.SystemPrompt != null) entity.SystemPrompt = request.SystemPrompt;
            if (request.MaxTokens != null) entity.MaxTokens = request.MaxTokens.Value;
            if (request.Temperature != null) entity.Temperature = request.Temperature.Value;
            if (request.TopP != null) entity.TopP = request.TopP.Value;
            if (request.IsActive != null) entity.IsActive = request.IsActive.Value;
            if (request.Tags != null) entity.Tags = string.Join(", ", request.Tags);
            entity.UpdatedBy = userId;
        }

        /// <summary>Chỉ gán các trường metadata (Description, Category, Tags, IsActive, UpdatedBy).</summary>
        public static void ApplyMetadataOnly(this AiPrompt entity, UpdateAiPromptRequest request, Guid userId)
        {
            if (request.Description != null) entity.Description = request.Description;
            if (request.Category != null) entity.Category = request.Category.Value;
            if (request.Tags != null) entity.Tags = string.Join(", ", request.Tags);
            if (request.IsActive != null) entity.IsActive = request.IsActive.Value;
            entity.UpdatedBy = userId;
        }

        /// <summary>Merge request lên current (request ưu tiên khi có giá trị), tạo entity mới với Version và IsActive.</summary>
        public static AiPrompt ToNewVersionEntity(this AiPrompt current, UpdateAiPromptRequest request, string nextVersion)
        {
            return new AiPrompt
            {
                Name = current.Name,
                Category = request.Category ?? current.Category,
                Version = nextVersion,
                IsActive = true,
                Template = request.Template ?? current.Template,
                SystemPrompt = request.SystemPrompt ?? current.SystemPrompt,
                Description = request.Description ?? current.Description,
                Variables = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : current.Variables,
                DefaultParameters = request.DefaultParameters != null ? JsonSerializer.Serialize(request.DefaultParameters) : current.DefaultParameters,
                MaxTokens = request.MaxTokens ?? current.MaxTokens,
                Temperature = request.Temperature ?? current.Temperature,
                TopP = request.TopP ?? current.TopP,
                Tags = request.Tags != null ? string.Join(", ", request.Tags) : current.Tags,
            };
        }

        public static AiPromptResponse ToResponse(this AiPrompt entity)
        {
            return new AiPromptResponse
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Category = entity.Category,
                Template = entity.Template,
                Version = entity.Version,
                IsActive = entity.IsActive,
                Variables = !string.IsNullOrEmpty(entity.Variables)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Variables)
                    : null,
                DefaultParameters = !string.IsNullOrEmpty(entity.DefaultParameters)
                    ? JsonSerializer.Deserialize<Dictionary<string, object>>(entity.DefaultParameters)
                    : null,
                SystemPrompt = entity.SystemPrompt,
                MaxTokens = entity.MaxTokens,
                Temperature = entity.Temperature,
                TopP = entity.TopP,
                Tags = !string.IsNullOrEmpty(entity.Tags)
                    ? [.. entity.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)]
                    : null,
                CreatedBy = entity.CreatedBy,
                UpdatedBy = entity.UpdatedBy,
                CreatedAt = entity.CreatedAt,
                UpdatedAt = entity.UpdatedAt
            };
        }
    }
}
