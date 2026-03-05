namespace Beyond8.Common.Events.Integration;

public record AiUsageDailyAggregatedEvent(
    DateOnly SnapshotDate,
    List<AiUsageDailyItem> Items
);

public record AiUsageDailyItem(
    string Model,
    int Provider,
    int TotalInputTokens,
    int TotalOutputTokens,
    int TotalTokens,
    decimal TotalInputCost,
    decimal TotalOutputCost,
    decimal TotalCost,
    int UsageCount
);
