namespace Beyond8.Sale.Application.Dtos.Wallets;

public class PlatformWalletResponse
{
    public Guid Id { get; set; }
    public decimal AvailableBalance { get; set; }
    // Pending balance held in escrow for platform (PoC). If >0, some revenue is not yet available.
    public decimal PendingBalance { get; set; }
    // Earliest time any pending platform funds become available.
    public DateTime? NextAvailableAt { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCouponCost { get; set; }
    public string Currency { get; set; } = "VND";
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
