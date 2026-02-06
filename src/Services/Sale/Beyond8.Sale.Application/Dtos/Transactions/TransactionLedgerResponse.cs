namespace Beyond8.Sale.Application.Dtos.Transactions;

public class TransactionLedgerResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? PayoutId { get; set; }
    public string? TransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}