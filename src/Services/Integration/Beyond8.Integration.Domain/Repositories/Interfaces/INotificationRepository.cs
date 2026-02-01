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

        /// <summary>
        /// Get notifications for a specific dashboard context (Student or Instructor).
        /// Returns notifications that match the context OR are General (appear everywhere).
        /// </summary>
        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByContextAsync(
            Guid userId,
            NotificationContext context,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null);

        /// <summary>
        /// Get unread notification count for a specific context.
        /// </summary>
        Task<int> GetUnreadCountByContextAsync(Guid userId, NotificationContext context);

        /// <summary>
        /// Get notifications by target (for Staff - AllStaff).
        /// </summary>
        Task<(List<Notification> Items, int TotalCount)> GetNotificationsByTargetAsync(
            NotificationTarget target,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            bool? isRead = null);

        /// <summary>
        /// Get unread count by target.
        /// </summary>
        Task<int> GetUnreadCountByTargetAsync(NotificationTarget target);

        /// <summary>
        /// Get all notifications in system for Admin log.
        /// </summary>
        Task<(List<Notification> Items, int TotalCount)> GetAllNotificationsAsync(
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null);
    }
}
