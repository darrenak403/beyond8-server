namespace Beyond8.Sale.Application.Dtos.Transactions;

public class CreateTransactionRequest
{
    public Guid UserId { get; set; }
    public string Type { get; set; } = string.Empty; // Payment, Refund, Payout, etc.
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid? OrderId { get; set; }
    public Guid? PayoutId { get; set; }
    public string? TransactionId { get; set; }
}