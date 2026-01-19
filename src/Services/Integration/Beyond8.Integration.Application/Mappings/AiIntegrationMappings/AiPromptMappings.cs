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
            Variables = request.Variables,
            DefaultParameters = request.DefaultParameters,
            SystemPrompt = request.SystemPrompt,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            Tags = request.Tags,
            CreatedBy = userId
        };
    }

    public static void UpdateFromRequest(this AiPrompt entity, UpdateAiPromptRequest request, Guid userId)
    {
        if (request.Name != null) entity.Name = request.Name;
        if (request.Description != null) entity.Description = request.Description;
        if (request.Category != null) entity.Category = request.Category.Value;
        if (request.Template != null) entity.Template = request.Template;
        if (request.Variables != null) entity.Variables = request.Variables;
        if (request.DefaultParameters != null) entity.DefaultParameters = request.DefaultParameters;
        if (request.SystemPrompt != null) entity.SystemPrompt = request.SystemPrompt;
        if (request.MaxTokens != null) entity.MaxTokens = request.MaxTokens.Value;
        if (request.Temperature != null) entity.Temperature = request.Temperature.Value;
        if (request.TopP != null) entity.TopP = request.TopP.Value;
        if (request.IsActive != null) entity.IsActive = request.IsActive.Value;
        if (request.Tags != null) entity.Tags = request.Tags;
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
            Variables = entity.Variables,
            DefaultParameters = entity.DefaultParameters,
            SystemPrompt = entity.SystemPrompt,
            MaxTokens = entity.MaxTokens,
            Temperature = entity.Temperature,
            TopP = entity.TopP,
            Tags = entity.Tags,
            CreatedBy = entity.CreatedBy,
            UpdatedBy = entity.UpdatedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
