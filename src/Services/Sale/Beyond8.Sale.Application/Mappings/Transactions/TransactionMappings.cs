using Beyond8.Sale.Application.Dtos.Transactions;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Mappings.Transactions;

public static class TransactionMappings
{
    public static TransactionLedgerResponse ToResponse(this TransactionLedger transaction)
    {
        return new TransactionLedgerResponse
        {
            Id = transaction.Id,
            WalletId = transaction.WalletId,
            Type = transaction.Type.ToString(),
            Status = transaction.Status.ToString(),
            Amount = transaction.Amount,
            Currency = transaction.Currency,
            BalanceBefore = transaction.BalanceBefore,
            BalanceAfter = transaction.BalanceAfter,
            ReferenceId = transaction.ReferenceId,
            ReferenceType = transaction.ReferenceType,
            Description = transaction.Description,
            ExternalTransactionId = transaction.ExternalTransactionId,
            CreatedAt = transaction.CreatedAt
        };
    }

    public static TransactionLedger ToEntity(this CreateTransactionRequest request, decimal balanceBefore, decimal balanceAfter)
    {
        return new TransactionLedger
        {
            WalletId = request.WalletId,
            Type = request.Type,
            Status = TransactionStatus.Completed,
            Amount = request.Amount,
            Currency = "VND",
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            ReferenceId = request.ReferenceId,
            ReferenceType = request.ReferenceType,
            Description = request.Description,
            CreatedAt = DateTime.UtcNow
        };
    }
}
