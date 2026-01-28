namespace Beyond8.Integration.Application.Dtos.Notifications
{
    public class InstructorNotificationResponse
    {
        public NotificationSection UserNotifications { get; set; } = new();
        public NotificationSection InstructorNotifications { get; set; } = new();
    }

    public class NotificationSection
    {
        public List<NotificationResponse> Items { get; set; } = [];
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
