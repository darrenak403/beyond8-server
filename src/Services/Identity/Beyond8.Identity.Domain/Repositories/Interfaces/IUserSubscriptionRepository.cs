using Beyond8.Common.Data.Interfaces;
using Beyond8.Identity.Domain.Entities;

namespace Beyond8.Identity.Domain.Repositories.Interfaces
{
    public interface IUserSubscriptionRepository : IGenericRepository<UserSubscription>
    {
        Task<UserSubscription?> GetActiveByUserIdAsync(Guid userId);
    }
}
