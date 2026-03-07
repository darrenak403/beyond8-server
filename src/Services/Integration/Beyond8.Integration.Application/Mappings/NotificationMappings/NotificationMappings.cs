using Beyond8.Common.Events.Assessment;
using Beyond8.Common.Events.Catalog;
using Beyond8.Common.Events.Identity;
using Beyond8.Common.Events.Learning;
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

        public static Notification OtpEmailEventToNotification(this OtpEmailEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Xác thực OTP",
                Message = data?.Message ?? $"Mã OTP cho {@event.Purpose} tại Beyond8 là: {@event.OtpCode}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.General
            };
        }

        public static Notification InstructorApprovalEventToNotification(this InstructorApprovalEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Đơn giảng viên được duyệt",
                Message = data?.Message ?? $"Chúc mừng! Đơn đăng ký giảng viên của bạn đã được duyệt. Xem hồ sơ: {@event.ProfileUrl}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Student
            };
        }

        public static Notification InstructorUpdateRequestEventToNotification(this InstructorUpdateRequestEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Yêu cầu cập nhật hồ sơ giảng viên",
                Message = data?.Message ?? $"Đơn đăng ký giảng viên của bạn cần được cập nhật. Ghi chú: {@event.UpdateNotes}",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification InstructorProfileSubmittedEventToNotification(this InstructorProfileSubmittedEvent @event, NotificationTarget target, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Đơn đăng ký giảng viên mới",
                Message = data?.Message ?? $"Giảng viên {@event.InstructorName} ({@event.Email}) đã gửi đơn đăng ký giảng viên mới.",
                UserId = Guid.Empty,
                Target = target,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Staff
            };
        }

        public static Notification ReLoginNotificationToNotification(this InstructorApprovalEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Tài khoản đã được duyệt",
                Message = data?.Message ?? "Tài khoản của bạn đã được duyệt thành công. Vui lòng đăng nhập lại để cập nhật quyền truy cập.",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Student
            };
        }

        public static Notification CourseRejectedEventToNotification(this CourseRejectedEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            var reason = @event.Reason ?? "Khóa học chưa đáp ứng đủ tiêu chuẩn chất lượng của Beyond8.";
            return new Notification
            {
                Title = data?.Title ?? "Khóa học bị từ chối",
                Message = data?.Message ?? $"Khóa học \"{@event.CourseName}\" của bạn đã bị từ chối. Lý do: {reason}",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification CourseApprovedEventToNotification(this CourseApprovedEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Khóa học được duyệt",
                Message = data?.Message ?? $"Chúc mừng! Khóa học \"{@event.CourseName}\" của bạn đã được duyệt và sẵn sàng để xuất bản.",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.Email],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification TranscodingVideoSuccessEventToNotification(this TranscodingVideoSuccessEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Video đã được upload",
                Message = data?.Message ?? $"Video của bài học {@event.LessonTitle} đã được upload thành công.",
                UserId = @event.InstructorId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Instructor
            };
        }

        public static Notification CourseCompletedEventToNotification(this CourseCompletedEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Chúc mừng hoàn thành khóa học",
                Message = data?.Message ?? $"Bạn đã hoàn thành khóa học \"{@event.CourseTitle}\". Bạn có thể xem chứng chỉ trong hồ sơ của mình.",
                UserId = @event.UserId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Student
            };
        }

        public static Notification AiAssignmentGradedEventToNotification(this AiAssignmentGradedEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Bài tập đã được chấm bởi AI",
                Message = data?.Message ?? $"Điểm số của bạn trong bài tập {@event.AssignmentTitle} là {@event.Score}.",
                UserId = @event.StudentId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Student
            };
        }

        public static Notification AssignmentGradedEventToNotification(this AssignmentGradedEvent @event, NotificationStatus status, DataInfor? data = null)
        {
            return new Notification
            {
                Title = data?.Title ?? "Bài tập đã được chấm bởi giảng viên",
                Message = data?.Message ?? $"Điểm số của bạn trong bài tập {@event.AssignmentTitle} là {@event.Score}.",
                UserId = @event.StudentId,
                Target = NotificationTarget.User,
                Status = status,
                Channels = [NotificationChannel.App],
                ReadAt = null,
                IsRead = false,
                Context = NotificationContext.Student
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
