using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Singleton wallet for the platform.
/// Tracks 30% commission revenue and absorbs system coupon costs.
/// AvailableBalance CAN go negative when system coupon cost exceeds current balance.
/// Negative balance is automatically offset by incoming 30% commissions.
/// </summary>
public class PlatformWallet : BaseEntity
{
    /// <summary>
    /// Current available balance. Can be negative when system coupon costs exceed revenue.
    /// Offset automatically by incoming 30% commissions.
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal AvailableBalance { get; set; } = 0;

    /// <summary>
    /// Lifetime accumulated revenue (only increases)
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalRevenue { get; set; } = 0;

    /// <summary>
    /// Lifetime total coupon costs absorbed by the platform
    /// </summary>
    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalCouponCost { get; set; } = 0;

    [MaxLength(10)]
    public string Currency { get; set; } = "VND";

    public bool IsActive { get; set; } = true;
}
