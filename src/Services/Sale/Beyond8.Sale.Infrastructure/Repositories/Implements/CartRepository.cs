using Beyond8.Sale.Domain.Entities;
using Beyond8.Sale.Domain.Repositories.Interfaces;
using Beyond8.Sale.Infrastructure.Data;
using Beyond8.Common.Data.Implements;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Sale.Infrastructure.Repositories.Implements;

public class CartRepository(SaleDbContext context) : PostgresRepository<Cart>(context), ICartRepository
{
    public async Task<IReadOnlyCollection<CartItem>> GetCartItemsByCourseIdAsync(Guid courseId)
    {
        return await context.CartItems
            .Where(ci => ci.CourseId == courseId && ci.DeletedAt == null)
            .ToListAsync();
    }
}
