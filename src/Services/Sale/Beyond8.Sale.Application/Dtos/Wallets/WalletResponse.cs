namespace Beyond8.Sale.Application.Dtos.Wallets;

public class InstructorWalletResponse
{
    public Guid InstructorId { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalEarned { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class WalletTransactionResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public string Type { get; set; } = string.Empty; // Credit or Debit
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}