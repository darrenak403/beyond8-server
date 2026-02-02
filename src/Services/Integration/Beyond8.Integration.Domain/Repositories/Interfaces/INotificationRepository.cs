using Beyond8.Common.Data.Interfaces;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Domain.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByContextAsync(
            Guid userId,
            NotificationContext context,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null);

        Task<int> GetUnreadCountByContextAsync(Guid userId, NotificationContext context);

        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByTargetAsync(
            NotificationTarget target,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            bool? isRead = null);

        Task<int> GetUnreadCountByTargetAsync(NotificationTarget target);

        Task<(List<Notification> Items, int TotalCount)> GetAllNotificationsAsync(
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null);
    }
}
