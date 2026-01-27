using Beyond8.Common.Utilities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Dtos.Notifications
{
    public class PaginationNotificationRequest : PaginationRequest
    {
        public NotificationStatus? Status { get; set; }
        public NotificationChannel? Channel { get; set; }
        public bool? IsRead { get; set; }
    }
}
