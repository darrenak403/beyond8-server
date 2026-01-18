using System.Linq.Expressions;
using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IUserRepository : IGenericRepository<User>
    {
    }
}