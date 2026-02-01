using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements
{
    public class NotificationRepository(IntegrationDbContext context) : PostgresRepository<Notification>(context), INotificationRepository
    {
        public async Task<(List<Notification> Items, int TotalCount)> GetNotificationsByContextAsync(
            Guid userId,
            NotificationContext context,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null)
        {
            var query = AsQueryable()
                .Where(n => n.UserId == userId &&
                    (n.Context == context || n.Context == NotificationContext.General) &&
                    n.Channels.Contains(NotificationChannel.App));

            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }

            if (channel.HasValue)
            {
                query = query.Where(n => n.Channels.Contains(channel.Value));
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetUnreadCountByContextAsync(Guid userId, NotificationContext context)
        {
            return await AsQueryable()
                .Where(n => n.UserId == userId &&
                    (n.Context == context || n.Context == NotificationContext.General) &&
                    n.IsRead == false &&
                    n.Channels.Contains(NotificationChannel.App) &&
                    n.Status == NotificationStatus.Delivered)
                .CountAsync();
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetNotificationsByTargetAsync(
            NotificationTarget target,
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            bool? isRead = null)
        {
            var query = AsQueryable()
                .Where(n => n.Target == target &&
                    n.Channels.Contains(NotificationChannel.App));

            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<int> GetUnreadCountByTargetAsync(NotificationTarget target)
        {
            return await AsQueryable()
                .Where(n => n.Target == target &&
                    n.IsRead == false &&
                    n.Channels.Contains(NotificationChannel.App) &&
                    n.Status == NotificationStatus.Delivered)
                .CountAsync();
        }

        public async Task<(List<Notification> Items, int TotalCount)> GetAllNotificationsAsync(
            int pageNumber,
            int pageSize,
            NotificationStatus? status = null,
            NotificationChannel? channel = null,
            bool? isRead = null)
        {
            var query = AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(n => n.Status == status.Value);
            }

            if (channel.HasValue)
            {
                query = query.Where(n => n.Channels.Contains(channel.Value));
            }

            if (isRead.HasValue)
            {
                query = query.Where(n => n.IsRead == isRead.Value);
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
