using System;

namespace Beyond8.Sale.Application.Dtos.Carts;

public class CartResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemResponse> Items { get; set; } = new();
    public decimal SubTotal { get; set; }
    public int TotalItems { get; set; }
}

