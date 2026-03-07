using System;

namespace Beyond8.Sale.Application.Dtos.Carts;

public class CartItemResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public decimal OriginalPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public DateTime? DiscountEndsAt { get; set; }
    public decimal FinalPrice { get; set; }
    public bool HasDiscount => DiscountPercent > 0 || DiscountAmount > 0;
}

