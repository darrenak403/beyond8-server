using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Dtos.Settlements;

public class UpcomingSettlementResponse
{
    public Guid TransactionId { get; set; }
    public Guid WalletId { get; set; }
    public Guid? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
    public DateTime? AvailableAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public TransactionStatus Status { get; set; }
}
