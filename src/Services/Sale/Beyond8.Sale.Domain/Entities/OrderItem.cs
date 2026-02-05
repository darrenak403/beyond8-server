using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Beyond8.Common.Data.Base;

namespace Beyond8.Sale.Domain.Entities;

public class OrderItem : BaseEntity
{
    public Guid OrderId { get; set; }

    [ForeignKey(nameof(OrderId))]
    public virtual Order Order { get; set; } = null!;

    public Guid CourseId { get; set; }

    [MaxLength(500)]
    public string CourseTitle { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal UnitPrice { get; set; }

    public int Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18, 2)")]
    public decimal LineTotal { get; set; }

    public Guid InstructorId { get; set; }
}
