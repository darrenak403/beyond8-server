using Beyond8.Common.Data.Interfaces;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Domain.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByUserAndRolesAsync(
            Guid userId,
            List<NotificationTarget> allowedTargets,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null);
    }
}
