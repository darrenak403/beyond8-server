namespace Beyond8.Sale.Application.Dtos.Settlements;

public class SettlementStatisticsResponse
{
    public int TotalOrdersSettled { get; set; }
    public int TotalOrdersPending { get; set; }
    public decimal TotalAmountSettled { get; set; }
    public decimal TotalAmountPending { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public List<InstructorSettlementSummary> ByInstructor { get; set; } = new();
    public List<DailySettlementSummary> ByDay { get; set; } = new();
}

public class InstructorSettlementSummary
{
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public int OrdersSettled { get; set; }
    public decimal TotalSettled { get; set; }
    public decimal AvgOrderValue { get; set; }
}

public class DailySettlementSummary
{
    public DateTime Date { get; set; }
    public int OrdersSettled { get; set; }
    public decimal TotalAmount { get; set; }
}
