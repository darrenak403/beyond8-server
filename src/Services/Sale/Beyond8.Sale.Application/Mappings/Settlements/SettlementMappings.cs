using Beyond8.Sale.Application.Dtos.Settlements;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Mappings.Settlements;

public static class SettlementMappings
{
    public static UpcomingSettlementResponse ToUpcomingSettlementResponse(
        this TransactionLedger ledger,
        string? orderNumber)
    {
        return new UpcomingSettlementResponse
        {
            TransactionId = ledger.Id,
            WalletId = ledger.WalletId,
            OrderId = ledger.ReferenceId,
            Amount = ledger.Amount,
            Currency = ledger.Currency,
            AvailableAt = ledger.AvailableAt,
            CreatedAt = ledger.CreatedAt,
            Status = ledger.Status,
            OrderNumber = orderNumber
        };
    }

    public static UpcomingByOrderResponse ToUpcomingByOrderResponse(
        Guid orderId,
        string? orderNumber,
        IEnumerable<(decimal Amount, DateTime? AvailableAt, TransactionStatus Status)> instructorTxs,
        IEnumerable<(decimal Amount, DateTime? EffectiveAt, TransactionStatus Status)> platformTxs)
    {
        var insList = instructorTxs.ToList();
        var platList = platformTxs.ToList();

        var insMin = insList.Any() ? insList.Min(x => x.AvailableAt) : null;
        var platMin = platList.Any() ? platList.Min(x => x.EffectiveAt) : null;

        var availableAt = insMin.HasValue && platMin.HasValue
            ? (insMin.Value <= platMin.Value ? insMin : platMin)
            : insMin ?? platMin;

        return new UpcomingByOrderResponse
        {
            OrderId = orderId,
            OrderNumber = orderNumber,
            InstructorAmount = insList.Sum(x => x.Amount),
            PlatformAmount = platList.Sum(x => x.Amount),
            AvailableAt = availableAt,
            Currency = "VND",
            InstructorStatus = insList.Any()
                ? (insList.All(x => x.Status == TransactionStatus.Completed) ? TransactionStatus.Completed : TransactionStatus.Pending)
                : null,
            PlatformStatus = platList.Any()
                ? (platList.All(x => x.Status == TransactionStatus.Completed) ? TransactionStatus.Completed : TransactionStatus.Pending)
                : null
        };
    }
}
