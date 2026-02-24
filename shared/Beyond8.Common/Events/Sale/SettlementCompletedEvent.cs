using System;

namespace Beyond8.Common.Events.Sale;

public record SettlementCompletedEvent(
    Guid OrderId,
    Guid InstructorId,
    decimal Amount,
    DateTime SettledAt
);
