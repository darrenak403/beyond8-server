using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

/// <summary>
/// Shopping cart entity - one cart per user (server-side cart for cross-device sync)
/// </summary>
public class Cart : BaseEntity
{
    // User Reference (Logical - No FK, cross-service)
    public Guid UserId { get; set; }

    // Navigation Properties
    public virtual ICollection<CartItem> CartItems { get; set; } = [];
}

/// <summary>
/// Individual item in a shopping cart (snapshot of course data at time of adding)
/// </summary>
public class CartItem : BaseEntity
{
    // Cart Reference
    public Guid CartId { get; set; }

    [ForeignKey(nameof(CartId))]
    public virtual Cart Cart { get; set; } = null!;

    // Course Snapshot (from Catalog Service)
    public Guid CourseId { get; set; }

    [Required, MaxLength(300)]
    public string CourseTitle { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? CourseThumbnail { get; set; }

    public Guid InstructorId { get; set; }

    [Required, MaxLength(200)]
    public string InstructorName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal OriginalPrice { get; set; }
}
