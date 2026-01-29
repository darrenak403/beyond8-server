using Microsoft.EntityFrameworkCore;
using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements;

public class UserRepository(IdentityDbContext context) : PostgresRepository<User>(context), IUserRepository
{
    public async Task<(List<User> Items, int TotalCount)> SearchUsersPagedAsync(
        int pageNumber,
        int pageSize,
        string? email,
        string? fullName,
        string? phoneNumber,
        string? specialization,
        string? address,
        bool? isEmailVerified,
        string? role,
        bool? isDescending)
    {
        IQueryable<User> query = AsQueryable()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role);

        if (!string.IsNullOrEmpty(email))
            query = query.Where(u => u.Email.ToLower().Contains(email.ToLower()));

        if (!string.IsNullOrEmpty(fullName))
            query = query.Where(u => u.FullName.ToLower().Contains(fullName.ToLower()));

        if (!string.IsNullOrEmpty(phoneNumber))
            query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));

        if (!string.IsNullOrEmpty(specialization))
            query = query.Where(u => u.Specialization != null && u.Specialization.ToLower().Contains(specialization.ToLower()));

        if (!string.IsNullOrEmpty(address))
            query = query.Where(u => u.Address != null && u.Address.ToLower().Contains(address.ToLower()));

        if (isEmailVerified != null)
            query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);

        if (!string.IsNullOrEmpty(role))
            query = query.Where(u => u.UserRoles.Any(ur => ur.Role.Code.ToLower() == role.ToLower() && ur.RevokedAt == null));

        query = isDescending == true
            ? query.OrderByDescending(u => u.CreatedAt)
            : query.OrderBy(u => u.CreatedAt);

        var totalCount = await query.CountAsync();

        var users = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

}
