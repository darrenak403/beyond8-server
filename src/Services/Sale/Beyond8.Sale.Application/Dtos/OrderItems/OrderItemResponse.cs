namespace Beyond8.Sale.Application.Dtos.OrderItems;

public class OrderItemResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }

    public Guid InstructorId { get; set; }
    public string InstructorName { get; set; } = string.Empty;

    // Pricing
    public decimal OriginalPrice { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal InstructorDiscountAmount { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }
}
