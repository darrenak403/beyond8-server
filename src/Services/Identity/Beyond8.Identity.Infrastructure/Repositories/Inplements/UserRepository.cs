using Beyond8.Common.Data.Implements;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;
using Beyond8.Identity.Domain.Repositories.Interfaces;
using Beyond8.Identity.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Identity.Infrastructure.Repositories.Inplements;

public class UserRepository(IdentityDbContext context) : PostgresRepository<User>(context), IUserRepository
{
    public async Task<(List<User> Items, int TotalCount)> SearchUsersPagedAsync(int pageNumber, int pageSize, string? email, string? fullName, string? phoneNumber, string? specialization, string? address, bool? isEmailVerified, UserRole? role, bool? isDescending)
    {
        var query = AsQueryable();

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(u => u.Email.Contains(email));
        }
        if (!string.IsNullOrEmpty(fullName))
        {
            query = query.Where(u => u.FullName.Contains(fullName));
        }
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            query = query.Where(u => string.IsNullOrWhiteSpace(phoneNumber) || u.PhoneNumber!.Contains(phoneNumber));
        }
        if (!string.IsNullOrEmpty(specialization))
        {
            query = query.Where(u => u.Specialization != null && u.Specialization.Contains(specialization));
        }
        if (!string.IsNullOrEmpty(address))
        {
            query = query.Where(u => u.Address != null && u.Address.Contains(address));
        }
        if (isEmailVerified != null)
        {
            query = query.Where(u => u.IsEmailVerified == isEmailVerified.Value);
        }
        if (role != null)
        {
            query = query.Where(u => u.Roles.Contains(role.Value));
        }
        if (isDescending != null)
        {
            query = query.OrderByDescending(u => u.CreatedAt);
        }

        var totalCount = await query.CountAsync();
        var users = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return (users, totalCount);
    }
}
