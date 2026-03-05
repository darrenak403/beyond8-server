using Beyond8.Analytic.Domain.Entities;
using Beyond8.Analytic.Domain.Repositories.Interfaces;
using Beyond8.Common.Events.Integration;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Analytic.Application.Consumers.Integration;

public class AiUsageDailyAggregatedEventConsumer(
    ILogger<AiUsageDailyAggregatedEventConsumer> logger,
    IUnitOfWork unitOfWork) : IConsumer<AiUsageDailyAggregatedEvent>
{
    public async Task Consume(ConsumeContext<AiUsageDailyAggregatedEvent> context)
    {
        var message = context.Message;

        var existing = await unitOfWork.AggAiUsageDailyRepository.GetBySnapshotDateAsync(message.SnapshotDate);
        foreach (var e in existing)
            await unitOfWork.AggAiUsageDailyRepository.DeleteAsync(e.Id);

        foreach (var item in message.Items)
        {
            var entity = new AggAiUsageDaily
            {
                SnapshotDate = message.SnapshotDate,
                Model = item.Model,
                Provider = item.Provider,
                TotalInputTokens = item.TotalInputTokens,
                TotalOutputTokens = item.TotalOutputTokens,
                TotalTokens = item.TotalTokens,
                TotalInputCost = item.TotalInputCost,
                TotalOutputCost = item.TotalOutputCost,
                TotalCost = item.TotalCost,
                UsageCount = item.UsageCount,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.Empty
            };
            await unitOfWork.AggAiUsageDailyRepository.AddAsync(entity);
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Persisted AI usage daily aggregation for {Date}: {Count} model(s)",
            message.SnapshotDate, message.Items.Count);
    }
}
