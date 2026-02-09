using Beyond8.Sale.Application.Dtos.Carts;
using Beyond8.Sale.Domain.Entities;

namespace Beyond8.Sale.Application.Mappings.Carts;

public static class CartMappings
{
    public static CartResponse ToResponse(this Cart cart)
    {
        var items = cart.CartItems.Select(ci => ci.ToResponse()).ToList();
        var originalTotal = items.Sum(i => i.OriginalPrice);
        var finalTotal = items.Sum(i => i.FinalPrice);

        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            Items = items,
            OriginalTotal = originalTotal,
            TotalDiscount = originalTotal - finalTotal,
            SubTotal = finalTotal,
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
            OriginalPrice = cartItem.OriginalPrice,
            DiscountPercent = cartItem.DiscountPercent,
            DiscountAmount = cartItem.DiscountAmount,
            DiscountEndsAt = cartItem.DiscountEndsAt,
            FinalPrice = cartItem.FinalPrice
        };
    }
}
