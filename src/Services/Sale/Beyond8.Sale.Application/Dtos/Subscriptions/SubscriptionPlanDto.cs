using System;
using System.Collections.Generic;

namespace Beyond8.Sale.Application.Dtos.Subscriptions;

public class SubscriptionPlanDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int DurationDays { get; set; }
    public int TotalRequestsInPeriod { get; set; }
    public int MaxRequestsPerWeek { get; set; }
    public List<string> Includes { get; set; } = [];
}
