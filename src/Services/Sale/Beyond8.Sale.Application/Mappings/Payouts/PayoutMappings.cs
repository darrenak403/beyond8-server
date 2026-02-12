using Beyond8.Sale.Application.Dtos.Payouts;
using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Enums;

namespace Beyond8.Sale.Application.Mappings.Payouts;

public static class PayoutMappings
{
    public static PayoutRequestResponse ToResponse(this PayoutRequest payout)
    {
        return new PayoutRequestResponse
        {
            Id = payout.Id,
            InstructorId = payout.InstructorId,
            WalletId = payout.WalletId,
            RequestNumber = payout.RequestNumber,
            Amount = payout.Amount,
            Currency = payout.Currency,
            Status = payout.Status,
            BankName = payout.BankName,
            BankAccountNumber = payout.BankAccountNumber,
            BankAccountName = payout.BankAccountName,
            Note = payout.Note,
            RequestedAt = payout.RequestedAt,
            ApprovedAt = payout.ApprovedAt,
            ApprovedBy = payout.ApprovedBy,
            ProcessedAt = payout.ProcessedAt,
            RejectedAt = payout.RejectedAt,
            RejectedBy = payout.RejectedBy,
            RejectionReason = payout.RejectionReason,
            ExternalTransactionId = payout.ExternalTransactionId,
            CreatedAt = payout.CreatedAt
        };
    }

    public static PayoutRequest ToEntity(this CreatePayoutRequest request, Guid walletId)
    {
        return new PayoutRequest
        {
            InstructorId = request.InstructorId,
            WalletId = walletId,
            RequestNumber = GeneratePayoutRequestNumber(),
            Amount = request.Amount,
            Currency = "VND",
            Status = PayoutStatus.Requested,
            BankName = request.BankName,
            BankAccountNumber = request.BankAccountNumber,
            BankAccountName = request.BankAccountName,
            Note = request.Note,
            RequestedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static string GeneratePayoutRequestNumber()
    {
        return $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
    }
}
