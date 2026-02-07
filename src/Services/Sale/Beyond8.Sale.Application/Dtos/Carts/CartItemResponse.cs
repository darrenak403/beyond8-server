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
}

