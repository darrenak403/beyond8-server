using Beyond8.Common.Utilities;
using Beyond8.Integration.Application.Dtos.Notifications;
using Beyond8.Integration.Domain.Enums;

namespace Beyond8.Integration.Application.Services.Interfaces
{
    public interface INotificationHistoryService
    {
        Task<ApiResponse<List<NotificationResponse>>> GetMyNotificationsAsync(
            Guid userId,
            List<string> userRoles,
            PaginationNotificationRequest pagination);

        Task<ApiResponse<List<NotificationResponse>>> GetNotificationsByContextAsync(
            Guid userId,
            NotificationContext context,
            PaginationNotificationRequest pagination);

        Task<ApiResponse<InstructorNotificationResponse>> GetInstructorNotificationsAsync(
            Guid userId,
            PaginationNotificationRequest pagination);

        Task<ApiResponse<bool>> UnreadNotificationAsync(Guid id, Guid userId);
        Task<ApiResponse<bool>> ReadNotificationAsync(Guid id, Guid userId);
        Task<ApiResponse<bool>> ReadAllNotificationAsync(Guid userId, NotificationContext? context = null);
        Task<ApiResponse<bool>> DeleteAllNotificationAsync(Guid userId, NotificationContext? context = null);
        Task<ApiResponse<bool>> DeleteNotificationAsync(Guid id, Guid userId);
        Task<ApiResponse<NotificationStatusResponse>> GetNotificationStatusAsync(Guid userId, NotificationContext? context = null);

        Task<ApiResponse<List<NotificationResponse>>> GetStaffNotificationsAsync(
            Guid userId,
            PaginationNotificationRequest pagination);

        Task<ApiResponse<NotificationStatusResponse>> GetStaffNotificationStatusAsync(Guid userId);

        Task<ApiResponse<List<NotificationLogResponse>>> GetAllNotificationLogsAsync(
            PaginationNotificationRequest pagination);
    }
}
