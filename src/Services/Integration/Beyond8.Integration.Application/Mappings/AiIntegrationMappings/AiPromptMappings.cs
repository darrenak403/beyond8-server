using System.Text.Json;
using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Domain.Entities;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings;

public static class AiPromptMappings
{
    public static AiPrompt ToEntity(this CreateAiPromptRequest request, Guid userId)
    {
        return new AiPrompt
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Template = request.Template,
            Version = request.Version,
            IsActive = true,
            Variables = request.Variables != null ? JsonSerializer.Serialize(request.Variables) : null,
            DefaultParameters = request.DefaultParameters != null ? JsonSerializer.Serialize(request.DefaultParameters) : null,
            SystemPrompt = request.SystemPrompt,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            Tags = request.Tags != null ? string.Join(", ", request.Tags) : null,
            CreatedBy = userId
        };
    }

    public static void UpdateFromRequest(this AiPrompt entity, UpdateAiPromptRequest request, Guid userId)
    {
        if (request.Name != null) entity.Name = request.Name;
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
                ? entity.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList()
                : null,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
