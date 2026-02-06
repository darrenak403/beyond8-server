using Beyond8.Sale.Application.Dtos.Carts;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Carts;

public static class CartMappings
{
    public static CartResponse ToResponse(this Cart cart)
    {
        var items = cart.CartItems.Select(ci => ci.ToResponse()).ToList();
        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items,
            SubTotal = items.Sum(i => i.OriginalPrice),
            TotalItems = items.Count
        };
    }

    public static CartItemResponse ToResponse(this CartItem cartItem)
    {
        return new CartItemResponse
        {
            Id = cartItem.Id,
            CourseId = cartItem.CourseId,
            CourseTitle = cartItem.CourseTitle,
            CourseThumbnail = cartItem.CourseThumbnail,
            InstructorId = cartItem.InstructorId,
            InstructorName = cartItem.InstructorName,
            OriginalPrice = cartItem.OriginalPrice
        };
    }
}
