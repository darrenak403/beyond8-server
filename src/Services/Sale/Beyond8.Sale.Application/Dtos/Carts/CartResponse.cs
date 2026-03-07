using System;

namespace Beyond8.Sale.Application.Dtos.Carts;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal OriginalTotal { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal SubTotal { get; set; }
    public int TotalItems { get; set; }
}

