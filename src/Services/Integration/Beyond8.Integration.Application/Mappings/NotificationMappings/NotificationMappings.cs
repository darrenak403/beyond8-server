using Beyond8.Common.Events.Catalog;
using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Mappings.NotificationMappings
{
    public static class NotificationMappings
    {

        public static NotificationResponse ToNotificationResponse(this Notification notification, Guid userId)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                UserId = notification.UserId == Guid.Empty ? userId : notification.UserId,
                Target = notification.Target,
                Status = notification.Status,
                Channels = notification.Channels,
                ReadAt = notification.ReadAt,
                IsRead = notification.IsRead,
                Context = notification.Context,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }

        public static Notification OtpEmailEventToNotification(this OtpEmailEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Xác thực OTP",
                Message = $"Mã OTP cho {@event.Purpose} tại Beyond8 là: {@event.OtpCode}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.General
            };
        }

        public static Notification InstructorApprovalEventToNotification(this InstructorApprovalEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Đơn giảng viên được duyệt",
                Message = $"Chúc mừng! Đơn đăng ký giảng viên của bạn đã được duyệt. Xem hồ sơ: {@event.ProfileUrl}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification InstructorUpdateRequestEventToNotification(this InstructorUpdateRequestEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Yêu cầu cập nhật hồ sơ giảng viên",
                Message = $"Đơn đăng ký giảng viên của bạn cần được cập nhật. Ghi chú: {@event.UpdateNotes}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification InstructorProfileSubmittedEventToNotification(this InstructorProfileSubmittedEvent @event, NotificationTarget target, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Đơn đăng ký giảng viên mới",
                Message = $"Giảng viên {@event.InstructorName} ({@event.Email}) đã gửi đơn đăng ký giảng viên mới.",
                UserId = Guid.Empty,
                Target = target,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Staff
            };
        }

        public static Notification ReLoginNotificationToNotification(this InstructorApprovalEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Yêu cầu đăng nhập lại",
                Message = "Tài khoản của bạn đã được duyệt thành công. Vui lòng đăng xuất và đăng nhập lại để cập nhật quyền truy cập.",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification CourseRejectedEventToNotification(this CourseRejectedEvent @event, NotificationStatus status)
        {
            var reason = @event.Reason ?? "Khóa học chưa đáp ứng đủ tiêu chuẩn chất lượng của Beyond8.";
            return new Notification
            {
                Title = "Khóa học bị từ chối",
                Message = $"Khóa học \"{@event.CourseName}\" của bạn đã bị từ chối. Lý do: {reason}",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification CourseApprovedEventToNotification(this CourseApprovedEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Khóa học được duyệt",
                Message = $"Chúc mừng! Khóa học \"{@event.CourseName}\" của bạn đã được duyệt và sẵn sàng để xuất bản.",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification TranscodingVideoSuccessEventToNotification(this TranscodingVideoSuccessEvent @event, NotificationStatus status)
        {
            return new Notification
            {
                Title = "Video đã được upload",
                Message = $"Video của bài học {@event.LessonTitle} đã được upload thành công.",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static NotificationLogResponse ToNotificationLogResponse(this Notification notification)
        {
            return new NotificationLogResponse
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Target = notification.Target,
                Status = notification.Status,
                Channels = notification.Channels,
                ReadAt = notification.ReadAt,
                IsRead = notification.IsRead,
                Context = notification.Context,
                CreatedAt = notification.CreatedAt,
                UpdatedAt = notification.UpdatedAt
            };
        }
    }
}
