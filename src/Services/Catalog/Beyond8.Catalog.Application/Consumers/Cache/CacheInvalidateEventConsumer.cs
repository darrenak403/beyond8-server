using Beyond8.Common.Events.Cache;
using Beyond8.Common.Caching;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Beyond8.Catalog.Application.Consumers.Cache;

public class CacheInvalidateEventConsumer(
    ILogger<CacheInvalidateEventConsumer> logger,
    ICacheService cacheService) : IConsumer<CacheInvalidateEvent>
{
    public async Task Consume(ConsumeContext<CacheInvalidateEvent> context)
    {
        var cacheKey = context.Message.CacheKey;

        await cacheService.RemoveAsync(cacheKey);

        logger.LogInformation("Invalidated cache key: {CacheKey}", cacheKey);
    }
}