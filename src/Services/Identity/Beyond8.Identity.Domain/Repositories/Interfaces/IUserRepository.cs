using System.Linq.Expressions;
using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;
using Beyond8.Identity.Domain.Enums;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
        Task<(List<User> Items, int TotalCount)> SearchUsersPagedAsync(
            int pageNumber,
            int pageSize,
            string? email,
            string? fullName,
            string? phoneNumber,
            string? specialization,
            string? address,
            bool? isEmailVerified,
            UserRole? role,
            bool? isDescending);
    }
}