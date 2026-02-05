namespace Beyond8.Sale.Application.Dtos.Settlements;

public class SettlementStatusResponse
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;

    public decimal InstructorEarnings { get; set; }
    public bool IsSettled { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime? SettlementEligibleAt { get; set; }
    public DateTime? SettledAt { get; set; }

    public int DaysUntilSettlement { get; set; }
    public bool IsEligibleNow { get; set; }

    public List<TransactionInfo> RelatedTransactions { get; set; } = new();
}

public class TransactionInfo
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? AvailableAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
