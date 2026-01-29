using Beyond8.Common.Data.Implements;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;
using Beyond8.Integration.Domain.Repositories.Interfaces;
using Beyond8.Integration.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Beyond8.Integration.Infrastructure.Repositories.Implements;

public class NotificationRepository(IntegrationDbContext context) : PostgresRepository<Notification>(context), INotificationRepository
{
    public async Task<(List<Notification> Items, int TotalCount)> GetNotificationsByUserAndRolesAsync(
        Guid userId,
        List<NotificationTarget> allowedTargets,
        int pageNumber,
        int pageSize,
        NotificationStatus? status = null,
        NotificationChannel? channel = null,
        bool? isRead = null)
    {
        var query = AsQueryable()
            .Where(n =>
                (n.UserId == userId && n.Target == NotificationTarget.User) ||
                (n.UserId == Guid.Empty && allowedTargets.Contains(n.Target)));

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
