using Beyond8.Sale.Application.Dtos.Wallets;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Wallets;

public static class PlatformWalletMappings
{
    public static PlatformWalletResponse ToResponse(this PlatformWallet wallet)
    {
        return new PlatformWalletResponse
        {
            Id = wallet.Id,
            AvailableBalance = wallet.AvailableBalance,
            TotalRevenue = wallet.TotalRevenue,
            TotalCouponCost = wallet.TotalCouponCost,
            Currency = wallet.Currency,
            IsActive = wallet.IsActive,
            CreatedAt = wallet.CreatedAt,
            UpdatedAt = wallet.UpdatedAt
        };
    }
}
