using Beyond8.Common.Data.Base;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Domain.Entities
{
    public class Notification : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public NotificationTarget Target { get; set; }
        public NotificationStatus Status { get; set; } 
        public List<NotificationChannel> Channels { get; set; } = [];
        public DateTime? ReadAt { get; set; }
        public bool? IsRead { get; set; } = null;
        public NotificationContext Context { get; set; } = NotificationContext.General;
    }
}