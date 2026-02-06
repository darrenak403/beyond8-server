using System;

namespace Beyond8.Sale.Application.Dtos.OrderItems;

public class OrderItemRequest
{
    public Guid CourseId { get; set; }
    public decimal Price { get; set; }
}