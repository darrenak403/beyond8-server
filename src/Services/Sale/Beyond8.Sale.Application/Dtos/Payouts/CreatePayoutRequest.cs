namespace Beyond8.Sale.Application.Dtos.Payouts;

public class CreatePayoutRequest
{
    public Guid InstructorId { get; set; }
    public decimal Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public string? Note { get; set; }
}