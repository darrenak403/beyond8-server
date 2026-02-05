namespace Beyond8.Sale.Application.Dtos.Orders;

public class OrderStatisticsResponse
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }

    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public decimal TotalInstructorEarnings { get; set; }

    public decimal AverageOrderValue { get; set; }

    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }

    public List<OrderStatusBreakdown> StatusBreakdown { get; set; } = new();
    public List<MonthlyRevenue> MonthlyData { get; set; } = new();
}

public class OrderStatusBreakdown
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalAmount { get; set; }
}

public class MonthlyRevenue
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
}
