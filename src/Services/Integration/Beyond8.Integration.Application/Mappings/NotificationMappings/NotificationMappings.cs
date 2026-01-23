using Beyond8.Common.Events.Identity;
using Beyond8.Integration.Domain.Entities;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Mappings.NotificationMappings;

public static class NotificationMappings
{
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
            IsRead = false
        };
    }

    public static Notification InstructorApprovalEmailEventToNotification(this InstructorApprovalEmailEvent @event, NotificationStatus status)
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
            IsRead = false
        };
    }

    public static Notification InstructorUpdateRequestEmailEventToNotification(this InstructorUpdateRequestEmailEvent @event, NotificationStatus status)
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
            IsRead = false
        };
    }

    public static Notification InstructorApplicationSubmittedEventToNotification(this InstructorApplicationSubmittedEvent @event, NotificationTarget target, NotificationStatus status)
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
            IsRead = false
        };
    }
}
