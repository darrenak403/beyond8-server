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
        var repo = unitOfWork.AggAiUsageDailyRepository;
        var existingList = await repo.GetBySnapshotDateAsync(message.SnapshotDate);

        foreach (var item in message.Items)
        {
            var existing = existingList.FirstOrDefault(e =>
                e.Model == item.Model && e.Provider == item.Provider);

            if (existing != null)
            {
                existing.TotalInputTokens = item.TotalInputTokens;
                existing.TotalOutputTokens = item.TotalOutputTokens;
                existing.TotalTokens = item.TotalTokens;
                existing.TotalInputCost = item.TotalInputCost;
                existing.TotalOutputCost = item.TotalOutputCost;
                existing.TotalCost = item.TotalCost;
                existing.UsageCount = item.UsageCount;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = Guid.Empty;
                await repo.UpdateAsync(existing.Id, existing);
            }
            else
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
                await repo.AddAsync(entity);
            }
        }

        await unitOfWork.SaveChangesAsync();

        logger.LogInformation(
            "Persisted AI usage daily aggregation for {Date}: {Count} model(s)",
            message.SnapshotDate, message.Items.Count);
    }
}
