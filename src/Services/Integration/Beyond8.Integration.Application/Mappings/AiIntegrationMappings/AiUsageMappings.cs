using Beyond8.Integration.Application.Dtos.AiIntegration;
using Beyond8.Integration.Domain.Entities;

namespace Beyond8.Integration.Application.Mappings.AiIntegrationMappings;

public static class AiUsageMappings
{
    public static AiUsage ToEntity(this AiUsageRequest request)
    {
        return new AiUsage
        {
            UserId = request.UserId,
            Provider = request.Provider,
            Model = request.Model,
            Operation = request.Operation,
            InputTokens = request.InputTokens,
            OutputTokens = request.OutputTokens,
            InputCost = request.InputCost,
            OutputCost = request.OutputCost,
            PromptId = request.PromptId,
            RequestSummary = request.RequestSummary,
            ResponseTimeMs = request.ResponseTimeMs,
            Status = request.Status,
            ErrorMessage = request.ErrorMessage,
            Metadata = request.Metadata
        };
    }

    public static AiUsageResponse ToResponse(this AiUsage entity)
    {
        return new AiUsageResponse
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Provider = entity.Provider,
            Model = entity.Model,
            Operation = entity.Operation,
            InputTokens = entity.InputTokens,
            OutputTokens = entity.OutputTokens,
            TotalTokens = entity.TotalTokens,
            InputCost = entity.InputCost,
            OutputCost = entity.OutputCost,
            TotalCost = entity.TotalCost,
            PromptId = entity.PromptId,
            RequestSummary = entity.RequestSummary,
            ResponseTimeMs = entity.ResponseTimeMs,
            Status = entity.Status,
            ErrorMessage = entity.ErrorMessage,
            Metadata = entity.Metadata,
            CreatedAt = entity.CreatedAt
        };
    }
}
