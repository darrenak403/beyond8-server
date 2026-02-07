using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Transactions;

public class CreateTransactionRequest
{
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; } // "Order", "Payout"
}