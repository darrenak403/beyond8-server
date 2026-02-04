namespace Beyond8.Sale.Application.Dtos.Payouts;

public class CreatePayoutRequest
{
    public Guid InstructorId { get; set; }
    public decimal Amount { get; set; }
    public string BankAccount { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string? Notes { get; set; }
}