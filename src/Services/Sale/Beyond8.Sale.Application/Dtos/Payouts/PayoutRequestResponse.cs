using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Payouts;

public class PayoutRequestResponse
{
    public Guid Id { get; set; }
    public Guid InstructorId { get; set; }
    public Guid WalletId { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public PayoutStatus Status { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankAccountNumber { get; set; } = string.Empty;
    public string BankAccountName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public string? RejectionReason { get; set; }
    public string? ExternalTransactionId { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}