using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Wallets;

public class PlatformWalletTransactionResponse
{
    public Guid Id { get; set; }
    public Guid PlatformWalletId { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public PlatformTransactionType Type { get; set; }
    public TransactionStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string? Description { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}