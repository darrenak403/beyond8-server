using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Wallets;

public static class WalletMappings
{
    public static InstructorWalletResponse ToResponse(this InstructorWallet wallet)
    {
        return new InstructorWalletResponse
        {
            Id = wallet.Id,
            InstructorId = wallet.InstructorId,
            AvailableBalance = wallet.AvailableBalance,
            HoldBalance = wallet.HoldBalance,
            Currency = wallet.Currency,
            TotalEarnings = wallet.TotalEarnings,
            TotalWithdrawn = wallet.TotalWithdrawn,
            LastPayoutAt = wallet.LastPayoutAt,
            IsActive = wallet.IsActive,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }

    public static WalletTransactionResponse ToTransactionResponse(this TransactionLedger transaction)
    {
        return new WalletTransactionResponse
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
}
