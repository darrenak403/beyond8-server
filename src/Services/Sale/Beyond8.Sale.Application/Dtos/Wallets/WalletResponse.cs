namespace Beyond8.Sale.Application.Dtos.Wallets;

public class InstructorWalletResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public decimal AvailableBalance { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal TotalEarnings { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public DateTime? LastPayoutAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WalletTransactionResponse
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public string? Description { get; set; }
    public string? ExternalTransactionId { get; set; }
    public DateTime CreatedAt { get; set; }
}