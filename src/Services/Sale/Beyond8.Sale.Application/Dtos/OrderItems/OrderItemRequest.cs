using System;

namespace Beyond8.Sale.Application.Dtos.OrderItems;

public class OrderItemRequest
{
    public Guid CourseId { get; set; }
    public string? InstructorCouponCode { get; set; } // Coupon riêng của instructor cho khóa học này
}